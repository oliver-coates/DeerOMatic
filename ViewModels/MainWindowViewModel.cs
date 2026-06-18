using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Deer_o_matic.Models;
using Deer_o_matic.Services;

namespace Deer_o_matic.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public FileUploadViewModel FileUpload { get; }

    public MainWindowViewModel(FileUploadViewModel fileUpload)
    {
        FileUpload = fileUpload;
    }
}
