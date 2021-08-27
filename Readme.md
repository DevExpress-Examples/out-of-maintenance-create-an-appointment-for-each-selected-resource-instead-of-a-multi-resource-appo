<!-- default badges list -->
![](https://img.shields.io/endpoint?url=https://codecentral.devexpress.com/api/v1/VersionRange/128633538/18.1.3%2B)
[![](https://img.shields.io/badge/Open_in_DevExpress_Support_Center-FF7200?style=flat-square&logo=DevExpress&logoColor=white)](https://supportcenter.devexpress.com/ticket/details/E3362)
[![](https://img.shields.io/badge/📖_How_to_use_DevExpress_Examples-e9f6fc?style=flat-square)](https://docs.devexpress.com/GeneralInformation/403183)
<!-- default badges end -->
# Create an appointment for each selected resource instead of a multi-resource appointment


<p>This approach can be used if you want to create several appointment copies instead of creating a multi-resource appointment when shared resources mode is used (see"Assigning Appointments to Resources" section in the <a href="http://documentation.devexpress.com/#WindowsForms/CustomDocument1756"><u>Resources for Appointments</u></a> help article). Also, you can simulate this mode i. e. you are not using shared resources in your application but simulate its usage with the approach described in this example.</p><p>The key idea of this approach is to create a custom appointment form (see <a href="http://documentation.devexpress.com/#WindowsForms/CustomDocument2288"><u>How to: Create a Custom EditAppointment Form with Custom Fields</u></a>) with <a href="http://documentation.devexpress.com/#WindowsForms/clsDevExpressXtraSchedulerUIAppointmentResourcesEdittopic"><u>AppointmentResourcesEdit</u></a> and define the following logic for it:</p>

```cs
public CustomAppointmentForm(SchedulerControl control, Appointment apt, bool openRecurrenceForm) {
    this.edResources.SchedulerControl = control;
}
void UpdateForm() {
    SuspendUpdate();
    try {
        ...
        edResources.ResourceIds.Clear();
        edResources.ResourceIds.Add(controller.ResourceId);
    }
    finally {
        ResumeUpdate();
    }
    UpdateIntervalControls();
}
private void btnOK_Click(object sender, System.EventArgs e) {
    ...
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
```

<p> </p><p>Note that we are using the <a href="http://documentation.devexpress.com/#CoreLibraries/DevExpressXtraSchedulerAppointment_Copytopic"><u>Appointment.Copy Method</u></a> to create appointment copies. This method works as required for appointment types that can be created via appointment form (<strong>Normal</strong> and <strong>Pattern</strong>).</p>

<br/>


