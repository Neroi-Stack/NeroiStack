function Component()
{
}

Component.prototype.createOperations = function()
{
    // Call default implementation to copy files
    component.createOperations();

    if (systemInfo.productType === "windows") {
        component.addOperation("CreateShortcut", "@TargetDir@/NeroiStack.exe", "@StartMenuDir@/NeroiStack.lnk", "workingDirectory=@TargetDir@");
        component.addOperation("CreateShortcut", "@TargetDir@/NeroiStack.exe", "@DesktopDir@/NeroiStack.lnk", "workingDirectory=@TargetDir@");
    }
}
