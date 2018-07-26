using System;
using System.Collections.Generic;
using System.Text;
using DevExpress.XtraScheduler;
using DevExpress.XtraScheduler.UI;
using System.Windows.Forms;

namespace SchedulerMultiResAppointments {
    public partial class CustomAppointmentForm : DevExpress.XtraEditors.XtraForm {
        SchedulerControl control;
        Appointment apt;
        bool openRecurrenceForm = false;
        int suspendUpdateCount;

        // The CustomAppointmentFormController class is inherited from
        // the AppointmentFormController to add custom properties.
        // See its declaration below.
        CustomAppointmentFormController controller;

        protected IAppointmentStorage Appointments {
            get { return control.DataStorage.Appointments; }
        }

        protected bool IsUpdateSuspended { 
            get { return suspendUpdateCount > 0; } 
        }

        public CustomAppointmentForm(SchedulerControl control, Appointment apt, bool openRecurrenceForm) {
            this.openRecurrenceForm = openRecurrenceForm;
            this.controller = new CustomAppointmentFormController(control, apt);
            this.apt = apt;
            this.control = control;
            
            // Required for Windows Form Designer support
            SuspendUpdate();
            InitializeComponent();
            ResumeUpdate();
            UpdateForm();
            
            // TODO: Add any constructor code after InitializeComponent call
            this.edResources.SchedulerControl = control;
        }

        #region Recurrence
        private void MyAppointmentEditForm_Activated(object sender, System.EventArgs e) {
            // Required to show the recurrence form.
            if (openRecurrenceForm) {
                openRecurrenceForm = false;
                OnRecurrenceButton();
            }
        }
        private void btnRecurrence_Click(object sender, System.EventArgs e) {
            OnRecurrenceButton();
        }

        void OnRecurrenceButton() {
            ShowRecurrenceForm();
        }

        void ShowRecurrenceForm() {
            if (!control.SupportsRecurrence)
                return;

            // Prepare to edit the appointment's recurrence.
            Appointment editedAptCopy = controller.EditedAppointmentCopy;
            Appointment editedPattern = controller.EditedPattern;
            Appointment patternCopy = controller.PrepareToRecurrenceEdit();

            AppointmentRecurrenceForm dlg = new AppointmentRecurrenceForm(patternCopy, control.OptionsView.FirstDayOfWeek, controller);

            // Required for skin support.
            dlg.LookAndFeel.ParentLookAndFeel = this.LookAndFeel.ParentLookAndFeel;

            DialogResult result = dlg.ShowDialog(this);
            dlg.Dispose();

            if (result == DialogResult.Abort)
                controller.RemoveRecurrence();
            else
                if (result == DialogResult.OK) {
                    controller.ApplyRecurrence(patternCopy);
                    if (controller.EditedAppointmentCopy != editedAptCopy)
                        UpdateForm();
                }
            UpdateIntervalControls();
        }

        #endregion

        #region Form control events

        private void dtStart_EditValueChanged(object sender, System.EventArgs e) {
            if (!IsUpdateSuspended)
                controller.DisplayStart = dtStart.DateTime.Date + timeStart.Time.TimeOfDay;
            UpdateIntervalControls();
        }

        private void timeStart_EditValueChanged(object sender, System.EventArgs e) {
            if (!IsUpdateSuspended)
                controller.DisplayStart = dtStart.DateTime.Date + timeStart.Time.TimeOfDay;
            UpdateIntervalControls();
        }
        private void timeEnd_EditValueChanged(object sender, System.EventArgs e) {
            if (IsUpdateSuspended) return;
            if (IsIntervalValid())
                controller.DisplayEnd = dtEnd.DateTime.Date + timeEnd.Time.TimeOfDay;
            else
                timeEnd.EditValue = new DateTime(controller.DisplayEnd.TimeOfDay.Ticks);

        }
        private void dtEnd_EditValueChanged(object sender, System.EventArgs e) {
            if (IsUpdateSuspended) return;
            if (IsIntervalValid())
                controller.DisplayEnd = dtEnd.DateTime.Date + timeEnd.Time.TimeOfDay;
            else
                dtEnd.EditValue = controller.DisplayEnd.Date;
        }
        bool IsIntervalValid() {
            DateTime start = dtStart.DateTime + timeStart.Time.TimeOfDay;
            DateTime end = dtEnd.DateTime + timeEnd.Time.TimeOfDay;
            return end >= start;
        }

        private void checkAllDay_CheckedChanged(object sender, System.EventArgs e) {
            controller.AllDay = this.checkAllDay.Checked;
            if (!IsUpdateSuspended)
                UpdateAppointmentStatus();

            UpdateIntervalControls();
        }
        #endregion

        #region Updating Form
        protected void SuspendUpdate() {
            suspendUpdateCount++;
        }
        protected void ResumeUpdate() {
            if (suspendUpdateCount > 0)
                suspendUpdateCount--;
        }

        void UpdateForm() {
            SuspendUpdate();
            try {
                txSubject.Text = controller.Subject;
                edStatus.AppointmentStatus = Appointments.Statuses.GetById(controller.StatusKey);
                edLabel.AppointmentLabel = Appointments.Labels.GetById(controller.LabelKey);

                dtStart.DateTime = controller.DisplayStart.Date;
                dtEnd.DateTime = controller.DisplayEnd.Date;

                timeStart.Time = new DateTime(controller.DisplayStart.TimeOfDay.Ticks);
                timeEnd.Time = new DateTime(controller.DisplayEnd.TimeOfDay.Ticks);
                checkAllDay.Checked = controller.AllDay;

                edStatus.Storage = control.Storage;
                edLabel.Storage = control.Storage;

                txPrice.Text = controller.Price.ToString();

                // Update resource selector
                edResources.ResourceIds.Clear();
                edResources.ResourceIds.Add(controller.ResourceId);
            }
            finally {
                ResumeUpdate();
            }
            UpdateIntervalControls();
        }

        protected virtual void UpdateIntervalControls() {
            if (IsUpdateSuspended)
                return;

            SuspendUpdate();
            
            try {
                dtStart.EditValue = controller.DisplayStart.Date;
                dtEnd.EditValue = controller.DisplayEnd.Date;
                timeStart.EditValue = new DateTime(controller.DisplayStart.TimeOfDay.Ticks);
                timeEnd.EditValue = new DateTime(controller.DisplayEnd.TimeOfDay.Ticks);

                timeStart.Visible = !controller.AllDay;
                timeEnd.Visible = !controller.AllDay;
                timeStart.Enabled = !controller.AllDay;
                timeEnd.Enabled = !controller.AllDay;
            }
            finally {
                ResumeUpdate();
            }
        }

        void UpdateAppointmentStatus() {
            IAppointmentStatus currentStatus = edStatus.AppointmentStatus;
            IAppointmentStatus newStatus = controller.UpdateStatus(currentStatus);
            
            if (newStatus != currentStatus)
                edStatus.AppointmentStatus = newStatus;
        }

        #endregion

        #region Save changes
        private void btnOK_Click(object sender, System.EventArgs e) {
            // Required to check the appointment for conflicts.
            if (!controller.IsConflictResolved())
                return;

            controller.Subject = txSubject.Text;
            controller.SetStatus(edStatus.AppointmentStatus);
            controller.SetLabel(edLabel.AppointmentLabel);
            controller.AllDay = this.checkAllDay.Checked;
            controller.DisplayStart = this.dtStart.DateTime.Date + this.timeStart.Time.TimeOfDay;
            controller.DisplayEnd = this.dtEnd.DateTime.Date + this.timeEnd.Time.TimeOfDay;
            controller.Price = Convert.ToDecimal(txPrice.Text);

            controller.ResourceId = edResources.ResourceIds[0];

            controller.ApplyChanges();

            foreach (object item in edResources.ResourceIds) {
                if (item.Equals(controller.ResourceId))
                    continue;
                
                Appointment apt = controller.EditedAppointmentCopy.Copy();

                apt.ResourceId = item;
                control.DataStorage.Appointments.Add(apt);
            }
        }
        #endregion

        #region CustomAppointmentFormController
        public class CustomAppointmentFormController : AppointmentFormController {
            public decimal Price {
                get {
                    object val = EditedAppointmentCopy.CustomFields["Field1"];

                    if (val == null || val == DBNull.Value)
                        return 0;
                    else
                        return (decimal)val;
                }
                set { 
                    EditedAppointmentCopy.CustomFields["Field1"] = value; 
                }
            }

            decimal SourcePrice { get { return (decimal)SourceAppointment.CustomFields["Field1"]; } set { SourceAppointment.CustomFields["Field1"] = value; } }

            public CustomAppointmentFormController(SchedulerControl control, Appointment apt)
                : base(control, apt) {
            }

            public override bool IsAppointmentChanged() {
                if (base.IsAppointmentChanged())
                    return true;
                return SourcePrice != Price;
            }

            protected override void ApplyCustomFieldsValues() {
                SourcePrice = Price;
            }
        }
        #endregion
    }

}
