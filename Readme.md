<!-- default file list -->
*Files to look at*:

* [CustomAppointmentForm.cs](./CS/CustomAppointmentForm.cs) (VB: [CustomAppointmentForm.vb](./VB/CustomAppointmentForm.vb))
* [Form1.cs](./CS/Form1.cs) (VB: [Form1.vb](./VB/Form1.vb))
* [Program.cs](./CS/Program.cs) (VB: [Program.vb](./VB/Program.vb))
<!-- default file list end -->
# Create an appointment for each selected resource instead of a multi-resource appointment


<p>This approach can be used if you want to create several appointment copies instead of creating a multi-resource appointment when shared resources mode is used (see"Assigning Appointments to Resources" section in the <a href="http://documentation.devexpress.com/#WindowsForms/CustomDocument1756"><u>Resources for Appointments</u></a> help article). Also, you can simulate this mode i. e. you are not using shared resources in your application but simulate its usage with the approach described in this example.</p><p>The key idea of this approach is to create a custom appointment form (see <a href="http://documentation.devexpress.com/#WindowsForms/CustomDocument2288"><u>How to: Create a Custom EditAppointment Form with Custom Fields</u></a>) with <a href="http://documentation.devexpress.com/#WindowsForms/clsDevExpressXtraSchedulerUIAppointmentResourcesEdittopic"><u>AppointmentResourcesEdit</u></a> and define the following logic for it:</p>

```cs
       public CustomAppointmentForm(SchedulerControl control, Appointment apt, bool openRecurrenceForm) {<newline/>           ...<newline/>
           this.edResources.SchedulerControl = control;<newline/>       }<newline/>
<newline/>
       void UpdateForm() {<newline/>           SuspendUpdate();<newline/>           try {<newline/>               ...<newline/>               edResources.ResourceIds.Clear();<newline/>               edResources.ResourceIds.Add(controller.ResourceId);<newline/>           }<newline/>           finally {<newline/>               ResumeUpdate();<newline/>           }<newline/>           UpdateIntervalControls();<newline/>       }<newline/>
<newline/>
       private void btnOK_Click(object sender, System.EventArgs e) {<newline/>           ...<newline/>           controller.ResourceId = edResources.ResourceIds[0];<newline/>           controller.ApplyChanges();<newline/>           foreach (object item in edResources.ResourceIds) {<newline/>               if (item.Equals(controller.ResourceId))<newline/>                   continue;<newline/>              <newline/>               Appointment apt = controller.EditedAppointmentCopy.Copy();
<newline/>               apt.ResourceId = item;<newline/>               control.Storage.Appointments.Add(apt);<newline/>           }<newline/>       }
```

<p> </p><p>Note that we are using the <a href="http://documentation.devexpress.com/#CoreLibraries/DevExpressXtraSchedulerAppointment_Copytopic"><u>Appointment.Copy Method</u></a> to create appointment copies. This method works as required for appointment types that can be created via appointment form (<strong>Normal</strong> and <strong>Pattern</strong>).</p>

<br/>


