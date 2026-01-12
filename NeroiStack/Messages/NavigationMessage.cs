using System;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace NeroiStack.Messages;

public class NavigationMessage(Type viewType) : ValueChangedMessage<Type>(viewType)
{
}
