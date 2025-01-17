using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BankMaui.Components.Layout.MainLayout;

namespace BankMaui.Components.Pages
{
    public partial class Admin_Home : ComponentBase
    {


        [CascadingParameter]
        public RequiredDetails requiredDetails { get; set; }

        public async Task Logout()
        {
            try
            {
                requiredDetails.CurrencyTypeUser = "";
                requiredDetails.CurrentUserUsername = "";
                await JS.InvokeVoidAsync("console.log", "logout success.");
                await JS.InvokeVoidAsync("showAlert", "Logout success.");
                Navigation.NavigateTo("/");
                return;
            }
            catch (Exception obj)
            {
                await JS.InvokeVoidAsync("console.log", "logout fail.");
                await JS.InvokeVoidAsync("console.log", "Exception occur.");
                await JS.InvokeVoidAsync("console.log", $"{obj.ToString()}");
                await JS.InvokeVoidAsync("showAlert", "Logout fail.");
                return;
            }

        }
    }
}
