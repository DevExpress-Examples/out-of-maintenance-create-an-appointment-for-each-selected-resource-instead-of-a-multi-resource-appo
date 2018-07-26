using System;
using System.Windows.Forms;
using DevExpress.XtraScheduler;

namespace SchedulerMultiResAppointments
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {
            schedulerControl1.GroupType = SchedulerGroupType.Resource;
            schedulerControl1.ActiveViewType = DevExpress.XtraScheduler.SchedulerViewType.Day;
            schedulerControl1.Views.DayView.ResourcesPerPage = 3;

            // TODO: This line of code loads data into the 'carsDBDataSet.Cars' table. You can move, or remove it, as needed.
            this.carsTableAdapter.Fill(this.carsDBDataSet.Cars);
            // TODO: This line of code loads data into the 'carsDBDataSet.CarScheduling' table. You can move, or remove it, as needed.
            this.carSchedulingTableAdapter.Fill(this.carsDBDataSet.CarScheduling);
        }

        private void schedulerControl1_EditAppointmentFormShowing(object sender, AppointmentFormEventArgs e) {
            Appointment apt = e.Appointment;

            // Required to open the recurrence form via context menu.
            bool openRecurrenceForm = apt.Type == AppointmentType.Pattern && !schedulerStorage1.Appointments.Items.Contains(apt);

            CustomAppointmentForm customForm = new CustomAppointmentForm(schedulerControl1, apt, openRecurrenceForm);

            customForm.LookAndFeel.ParentLookAndFeel = schedulerControl1.LookAndFeel;

            e.DialogResult = customForm.ShowDialog();
            schedulerControl1.Refresh();
            e.Handled = true;
        }
    }
}