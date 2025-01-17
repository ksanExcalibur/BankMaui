using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BankMaui.Models;
using static BankMaui.Components.Layout.MainLayout;
using System.Text.Json;
using System.Security.Cryptography;

namespace BankMaui.Components.Pages
{
    public partial class SigninPage : ComponentBase
    {

        string username = "";
        string password = "";
        string first_name = "";
        string last_name = "";
        float initial_available_balance = 0.0f;
        float initial_debt_balance = 0.0f;


        [CascadingParameter]
        public RequiredDetails requiredDetails { get; set; }




        private async Task<bool> IsUsernameInJsonFile(string username)
        {
            try
            {
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TransactionsMate");
                string filePath = Path.Combine(directoryPath, "user_data.json");

                if (!File.Exists(filePath))
                {
                    await JS.InvokeVoidAsync("console.log", "user_data.json file does not exist.");
                    return false;
                }

                string jsonData = await File.ReadAllTextAsync(filePath);
                var userList = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(jsonData);

                return userList?.Any(user => user.ContainsKey("Username") && user["Username"] == username) ?? false;
            }
            catch (Exception ex)
            {
                await JS.InvokeVoidAsync("console.log", $"Exception while checking username in JSON file: {ex.Message}");
                return false;
            }
        }


        public async Task SaveUsernameToJsonFile(string username)
        {
            try
            {
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TransactionsMate");
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                    await JS.InvokeVoidAsync("console.log", "Directory created at: " + directoryPath);
                }

                string filePath = Path.Combine(directoryPath, "user_data.json");

                // Initialize a user list
                List<Dictionary<string, string>> userList = new();

                // Read existing data if file exists
                if (File.Exists(filePath))
                {
                    string existingJson = await File.ReadAllTextAsync(filePath);
                    userList = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(existingJson) ?? new List<Dictionary<string, string>>();
                }

                // Check if the username already exists
                if (userList.Any(user => user.ContainsKey("Username") && user["Username"] == username))
                {
                    await JS.InvokeVoidAsync("console.log", "Username already exists.");
                    return;
                }

                // Add the new username
                userList.Add(new Dictionary<string, string> { { "Username", username } });

                // Save the updated list
                string jsonData = JsonSerializer.Serialize(userList, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(filePath, jsonData);

                await JS.InvokeVoidAsync("console.log", $"Username '{username}' saved successfully to: {filePath}");
            }
            catch (Exception ex)
            {
                await JS.InvokeVoidAsync("console.log", $"Failed to save username to JSON file: {ex.Message}");
            }
        }


        public string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }




        public async Task SignInUser()
        {
            try
            {
                await JS.InvokeVoidAsync("console.log", "requiredDetails user_info_list count");
                await JS.InvokeVoidAsync("console.log", $"{requiredDetails.user_info_list.Count()}");

                if (
                    String.IsNullOrEmpty(first_name) ||
                     String.IsNullOrEmpty(last_name) ||
                      String.IsNullOrEmpty(username) ||
                       String.IsNullOrEmpty(password)

                )
                {
                    await JS.InvokeVoidAsync("console.log", "Provide detail invalid.");
                    await JS.InvokeVoidAsync("showAlert", "Provide all details correctly.");
                    return;

                }

                if (requiredDetails.user_info_list.Count() > 0)
                {
                    var current_user_data = requiredDetails.user_info_list.FirstOrDefault(x => x.Username == username);

                    bool result = await IsUsernameInJsonFile(username.ToString());

                    await JS.InvokeVoidAsync("console.log", "Sign in result");
                    await JS.InvokeVoidAsync("console.log", $"{result}");

                    if (current_user_data != null || result == true)
                    {
                        await JS.InvokeVoidAsync("console.log", "Username same");
                        await JS.InvokeVoidAsync("showAlert", "Username already exists.Use different.");
                        return;
                    }
                    else
                    {
                        UserInfoMoodel obj = new UserInfoMoodel(
                            user_username: username,
                            user_userPassword: HashPassword(password),
                            user_AvailableBalance: initial_available_balance,
                            user_DebtBalance: initial_debt_balance,
                            user_firstName: first_name,
                            user_lastName: last_name,
                            user_type: "user");
                        requiredDetails.user_info_list.Add(obj);
                        await JS.InvokeVoidAsync("console.log", "Save user signin data in json file success.");
                        await SaveUsernameToJsonFile(username.ToString());
                        await JS.InvokeVoidAsync("console.log", "signin success");
                        // await JS.InvokeVoidAsync("console.log", $"{requiredDetails.user_info_list[0].Username}");
                        await JS.InvokeVoidAsync("console.log", "user_info_list count value");
                        await JS.InvokeVoidAsync("console.log", $"{requiredDetails.user_info_list.Count().ToString()}");
                        await JS.InvokeVoidAsync("showAlert", "Sign-in successful!");
                        Navigation.NavigateTo("/signinpage", forceLoad: false);
                        return;
                    }
                }
                else
                {

                    UserInfoMoodel obj = new UserInfoMoodel(
                        user_username: username,
                        user_userPassword: HashPassword(password),
                        user_AvailableBalance: initial_available_balance,
                        user_DebtBalance: initial_debt_balance,
                        user_firstName: first_name,
                        user_lastName: last_name,
                        user_type: "user");
                    requiredDetails.user_info_list.Add(obj);
                    await SaveUsernameToJsonFile(username.ToString());
                    await JS.InvokeVoidAsync("console.log", "Save user signin data in json file success.");
                    // await JS.InvokeVoidAsync("console.log", $"{requiredDetails.user_info_list[0].Username}");
                    await JS.InvokeVoidAsync("console.log", "user_info_list count value");
                    await JS.InvokeVoidAsync("console.log", $"{requiredDetails.user_info_list.Count().ToString()}");
                    await JS.InvokeVoidAsync("showAlert", "Sign-in successful!");
                    Navigation.NavigateTo("/signinpage", forceLoad: false);
                    return;
                }
            }
            catch (Exception obj)
            {
                await JS.InvokeVoidAsync("console.log", "signin fail");
                await JS.InvokeVoidAsync("console.log", "exception caught");
                await JS.InvokeVoidAsync("console.log", $"{obj.ToString}");
                await JS.InvokeVoidAsync("console.log", "{user_info_list count value}");
                await JS.InvokeVoidAsync("console.log", $"{requiredDetails.user_info_list.Count()}");
                await JS.InvokeVoidAsync("showAlert", "signin fail");

                return;
            }
        }
    }
}
