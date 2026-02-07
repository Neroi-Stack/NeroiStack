function Component()
{
}

Component.prototype.createOperations = function()
{
    // Call default implementation to copy files
    component.createOperations();

    if (systemInfo.productType === "windows") {
        component.addOperation("CreateShortcut", "@TargetDir@/NeroiStack.exe", "@StartMenuDir@/NeroiStack.lnk", "workingDirectory=@TargetDir@;icon=@TargetDir@/NeroiStack.ico");
        component.addOperation("CreateShortcut", "@TargetDir@/NeroiStack.exe", "@DesktopDir@/NeroiStack.lnk", "workingDirectory=@TargetDir@;icon=@TargetDir@/NeroiStack.ico");
    } else {
        var execPath = "@TargetDir@/NeroiStack";
        component.addOperation("CreateShortcut", execPath, "@StartMenuDir@/NeroiStack.desktop", "workingDirectory=@TargetDir@;icon=@TargetDir@/NeroiStack.png;type=Application");
        component.addOperation("CreateShortcut", execPath, "@DesktopDir@/NeroiStack.desktop", "workingDirectory=@TargetDir@;icon=@TargetDir@/NeroiStack.png;type=Application");
    }
}
