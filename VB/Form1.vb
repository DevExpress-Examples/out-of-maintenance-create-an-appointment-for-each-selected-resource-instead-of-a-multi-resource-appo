Imports System
Imports System.Windows.Forms
Imports DevExpress.XtraScheduler

Namespace SchedulerMultiResAppointments
    Partial Public Class Form1
        Inherits Form

        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub Form1_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
            schedulerControl1.GroupType = SchedulerGroupType.Resource
            schedulerControl1.ActiveViewType = DevExpress.XtraScheduler.SchedulerViewType.Day
            schedulerControl1.Views.DayView.ResourcesPerPage = 3

            ' TODO: This line of code loads data into the 'carsDBDataSet.Cars' table. You can move, or remove it, as needed.
            Me.carsTableAdapter.Fill(Me.carsDBDataSet_Renamed.Cars)
            ' TODO: This line of code loads data into the 'carsDBDataSet.CarScheduling' table. You can move, or remove it, as needed.
            Me.carSchedulingTableAdapter.Fill(Me.carsDBDataSet_Renamed.CarScheduling)
        End Sub

        Private Sub schedulerControl1_EditAppointmentFormShowing(ByVal sender As Object, ByVal e As AppointmentFormEventArgs) Handles schedulerControl1.EditAppointmentFormShowing
            Dim apt As Appointment = e.Appointment

            ' Required to open the recurrence form via context menu.
            Dim openRecurrenceForm As Boolean = apt.Type = AppointmentType.Pattern AndAlso Not schedulerStorage1.Appointments.Items.Contains(apt)

            Dim customForm As New CustomAppointmentForm(schedulerControl1, apt, openRecurrenceForm)

            customForm.LookAndFeel.ParentLookAndFeel = schedulerControl1.LookAndFeel

            e.DialogResult = customForm.ShowDialog()
            schedulerControl1.Refresh()
            e.Handled = True
        End Sub
    End Class
End Namespace