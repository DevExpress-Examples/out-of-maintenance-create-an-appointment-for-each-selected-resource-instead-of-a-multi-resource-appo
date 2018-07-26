Imports System
Imports System.Collections.Generic
Imports System.Text
Imports DevExpress.XtraScheduler
Imports DevExpress.XtraScheduler.UI
Imports System.Windows.Forms

Namespace SchedulerMultiResAppointments
    Partial Public Class CustomAppointmentForm
        Inherits DevExpress.XtraEditors.XtraForm

        Private control As SchedulerControl
        Private apt As Appointment
        Private openRecurrenceForm As Boolean = False
        Private suspendUpdateCount As Integer

        ' The CustomAppointmentFormController class is inherited from
        ' the AppointmentFormController to add custom properties.
        ' See its declaration below.
        Private controller As CustomAppointmentFormController

        Protected ReadOnly Property Appointments() As IAppointmentStorage
            Get
                Return control.DataStorage.Appointments
            End Get
        End Property

        Protected ReadOnly Property IsUpdateSuspended() As Boolean
            Get
                Return suspendUpdateCount > 0
            End Get
        End Property

        Public Sub New(ByVal control As SchedulerControl, ByVal apt As Appointment, ByVal openRecurrenceForm As Boolean)
            Me.openRecurrenceForm = openRecurrenceForm
            Me.controller = New CustomAppointmentFormController(control, apt)
            Me.apt = apt
            Me.control = control

            ' Required for Windows Form Designer support
            SuspendUpdate()
            InitializeComponent()
            ResumeUpdate()
            UpdateForm()

            ' TODO: Add any constructor code after InitializeComponent call
            Me.edResources.SchedulerControl = control
        End Sub

        #Region "Recurrence"
        Private Sub MyAppointmentEditForm_Activated(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Activated
            ' Required to show the recurrence form.
            If openRecurrenceForm Then
                openRecurrenceForm = False
                OnRecurrenceButton()
            End If
        End Sub
        Private Sub btnRecurrence_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnRecurrence.Click
            OnRecurrenceButton()
        End Sub

        Private Sub OnRecurrenceButton()
            ShowRecurrenceForm()
        End Sub

        Private Sub ShowRecurrenceForm()
            If Not control.SupportsRecurrence Then
                Return
            End If

            ' Prepare to edit the appointment's recurrence.
            Dim editedAptCopy As Appointment = controller.EditedAppointmentCopy
            Dim editedPattern As Appointment = controller.EditedPattern
            Dim patternCopy As Appointment = controller.PrepareToRecurrenceEdit()

            Dim dlg As New AppointmentRecurrenceForm(patternCopy, control.OptionsView.FirstDayOfWeek, controller)

            ' Required for skin support.
            dlg.LookAndFeel.ParentLookAndFeel = Me.LookAndFeel.ParentLookAndFeel

            Dim result As DialogResult = dlg.ShowDialog(Me)
            dlg.Dispose()

            If result = DialogResult.Abort Then
                controller.RemoveRecurrence()
            Else
                If result = DialogResult.OK Then
                    controller.ApplyRecurrence(patternCopy)
                    If controller.EditedAppointmentCopy IsNot editedAptCopy Then
                        UpdateForm()
                    End If
                End If
            End If
            UpdateIntervalControls()
        End Sub

        #End Region

        #Region "Form control events"

        Private Sub dtStart_EditValueChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles dtStart.EditValueChanged
            If Not IsUpdateSuspended Then
                controller.DisplayStart = dtStart.DateTime.Date + timeStart.Time.TimeOfDay
            End If
            UpdateIntervalControls()
        End Sub

        Private Sub timeStart_EditValueChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles timeStart.EditValueChanged
            If Not IsUpdateSuspended Then
                controller.DisplayStart = dtStart.DateTime.Date + timeStart.Time.TimeOfDay
            End If
            UpdateIntervalControls()
        End Sub
        Private Sub timeEnd_EditValueChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles timeEnd.EditValueChanged
            If IsUpdateSuspended Then
                Return
            End If
            If IsIntervalValid() Then
                controller.DisplayEnd = dtEnd.DateTime.Date + timeEnd.Time.TimeOfDay
            Else
                timeEnd.EditValue = New Date(controller.DisplayEnd.TimeOfDay.Ticks)
            End If

        End Sub
        Private Sub dtEnd_EditValueChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles dtEnd.EditValueChanged
            If IsUpdateSuspended Then
                Return
            End If
            If IsIntervalValid() Then
                controller.DisplayEnd = dtEnd.DateTime.Date + timeEnd.Time.TimeOfDay
            Else
                dtEnd.EditValue = controller.DisplayEnd.Date
            End If
        End Sub
        Private Function IsIntervalValid() As Boolean
            Dim start As Date = dtStart.DateTime + timeStart.Time.TimeOfDay
            Dim [end] As Date = dtEnd.DateTime + timeEnd.Time.TimeOfDay
            Return [end] >= start
        End Function

        Private Sub checkAllDay_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles checkAllDay.CheckedChanged
            controller.AllDay = Me.checkAllDay.Checked
            If Not IsUpdateSuspended Then
                UpdateAppointmentStatus()
            End If

            UpdateIntervalControls()
        End Sub
        #End Region

        #Region "Updating Form"
        Protected Sub SuspendUpdate()
            suspendUpdateCount += 1
        End Sub
        Protected Sub ResumeUpdate()
            If suspendUpdateCount > 0 Then
                suspendUpdateCount -= 1
            End If
        End Sub

        Private Sub UpdateForm()
            SuspendUpdate()
            Try
                txSubject.Text = controller.Subject
                edStatus.AppointmentStatus = Appointments.Statuses.GetById(controller.StatusKey)
                edLabel.AppointmentLabel = Appointments.Labels.GetById(controller.LabelKey)

                dtStart.DateTime = controller.DisplayStart.Date
                dtEnd.DateTime = controller.DisplayEnd.Date

                timeStart.Time = New Date(controller.DisplayStart.TimeOfDay.Ticks)
                timeEnd.Time = New Date(controller.DisplayEnd.TimeOfDay.Ticks)
                checkAllDay.Checked = controller.AllDay

                edStatus.Storage = control.Storage
                edLabel.Storage = control.Storage

                txPrice.Text = controller.Price.ToString()

                ' Update resource selector
                edResources.ResourceIds.Clear()
                edResources.ResourceIds.Add(controller.ResourceId)
            Finally
                ResumeUpdate()
            End Try
            UpdateIntervalControls()
        End Sub

        Protected Overridable Sub UpdateIntervalControls()
            If IsUpdateSuspended Then
                Return
            End If

            SuspendUpdate()

            Try
                dtStart.EditValue = controller.DisplayStart.Date
                dtEnd.EditValue = controller.DisplayEnd.Date
                timeStart.EditValue = New Date(controller.DisplayStart.TimeOfDay.Ticks)
                timeEnd.EditValue = New Date(controller.DisplayEnd.TimeOfDay.Ticks)

                timeStart.Visible = Not controller.AllDay
                timeEnd.Visible = Not controller.AllDay
                timeStart.Enabled = Not controller.AllDay
                timeEnd.Enabled = Not controller.AllDay
            Finally
                ResumeUpdate()
            End Try
        End Sub

        Private Sub UpdateAppointmentStatus()
            Dim currentStatus As IAppointmentStatus = edStatus.AppointmentStatus
            Dim newStatus As IAppointmentStatus = controller.UpdateStatus(currentStatus)

            If newStatus IsNot currentStatus Then
                edStatus.AppointmentStatus = newStatus
            End If
        End Sub

        #End Region

        #Region "Save changes"
        Private Sub btnOK_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnOK.Click
            ' Required to check the appointment for conflicts.
            If Not controller.IsConflictResolved() Then
                Return
            End If

            controller.Subject = txSubject.Text
            controller.SetStatus(edStatus.AppointmentStatus)
            controller.SetLabel(edLabel.AppointmentLabel)
            controller.AllDay = Me.checkAllDay.Checked
            controller.DisplayStart = Me.dtStart.DateTime.Date + Me.timeStart.Time.TimeOfDay
            controller.DisplayEnd = Me.dtEnd.DateTime.Date + Me.timeEnd.Time.TimeOfDay
            controller.Price = Convert.ToDecimal(txPrice.Text)

            controller.ResourceId = edResources.ResourceIds(0)

            controller.ApplyChanges()

            For Each item As Object In edResources.ResourceIds
                If item.Equals(controller.ResourceId) Then
                    Continue For
                End If

                Dim apt As Appointment = controller.EditedAppointmentCopy.Copy()

                apt.ResourceId = item
                control.DataStorage.Appointments.Add(apt)
            Next item
        End Sub
        #End Region

        #Region "CustomAppointmentFormController"
        Public Class CustomAppointmentFormController
            Inherits AppointmentFormController

            Public Property Price() As Decimal
                Get
                    Dim val As Object = EditedAppointmentCopy.CustomFields("Field1")

                    If val Is Nothing OrElse val Is DBNull.Value Then
                        Return 0
                    Else
                        Return DirectCast(val, Decimal)
                    End If
                End Get
                Set(ByVal value As Decimal)
                    EditedAppointmentCopy.CustomFields("Field1") = value
                End Set
            End Property

            Private Property SourcePrice() As Decimal
                Get
                    Return CDec(SourceAppointment.CustomFields("Field1"))
                End Get
                Set(ByVal value As Decimal)
                    SourceAppointment.CustomFields("Field1") = value
                End Set
            End Property

            Public Sub New(ByVal control As SchedulerControl, ByVal apt As Appointment)
                MyBase.New(control, apt)
            End Sub

            Public Overrides Function IsAppointmentChanged() As Boolean
                If MyBase.IsAppointmentChanged() Then
                    Return True
                End If
                Return SourcePrice <> Price
            End Function

            Protected Overrides Sub ApplyCustomFieldsValues()
                SourcePrice = Price
            End Sub
        End Class
        #End Region
    End Class

End Namespace
