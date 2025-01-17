using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static BankMaui.Components.Layout.MainLayout;

namespace BankMaui.Components.Pages
{
    public partial class Login:ComponentBase
    {
        public string username { get; set; } = "";
        public string password { get; set; } = "";
        public string currencyType { get; set; } = "";

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

        // Method to handle form submission
        private async Task LoginUser()
        {
            try
            {
                if (string.IsNullOrEmpty(username))
                {
                    await JS.InvokeVoidAsync("showAlert", "Enter username and try again.");
                    return;
                }

                if (string.IsNullOrEmpty(password))
                {
                    await JS.InvokeVoidAsync("showAlert", "Enter password and try again.");
                    return;
                }

                var user_details = requiredDetails.user_info_list.FirstOrDefault(x => x.Username == username);
                bool result = await IsUsernameInJsonFile(username);
                if (result == true || user_details.Username == username)
                {
                    if (user_details.UserPassword == HashPassword(password))
                    {
                        if (user_details.UserType == "user")
                        {
                            requiredDetails.CurrencyTypeUser = currencyType;
                            requiredDetails.CurrentUserUsername = username;
                            Dictionary<string, string> first_login_currency_type = new Dictionary<string, string>()
                                                       {
                                                            { "user_username", requiredDetails.CurrentUserUsername },
                                                            { "firstlogin_currency_type", requiredDetails.CurrencyTypeUser }
                                                       };
                            string keyToFind = "user_username";
                            string valueToMatch = requiredDetails.CurrentUserUsername;

                            if (requiredDetails.FirstLoginCurrencyType.Count() >= 1)
                            {
                                bool already_login = CheckUserFirstLogin(requiredDetails.FirstLoginCurrencyType, keyToFind, valueToMatch);
                                if (already_login == true)
                                {
                                    await JS.InvokeVoidAsync("console.log", "first login value");
                                    await JS.InvokeVoidAsync("console.log", $"{requiredDetails.FirstLoginCurrencyType.Count()}");
                                    await JS.InvokeVoidAsync("console.log", $"{requiredDetails.FirstLoginCurrencyType[0]["firstlogin_currency_type"]}");
                                    await JS.InvokeVoidAsync("console.log", "User currency type.");
                                    await JS.InvokeVoidAsync("console.log", $"{requiredDetails.CurrencyTypeUser.ToString()}");
                                    await JS.InvokeVoidAsync("console.log", "login success");

                                    var user_data = requiredDetails.user_info_list.FirstOrDefault(x => x.Username == requiredDetails.CurrentUserUsername);
                                    string first_login_currency_type_value = GetFirstLoginCurrencyType(requiredDetails.CurrentUserUsername);
                                    await JS.InvokeVoidAsync("console.log", "first_login_currency_type_value");
                                    await JS.InvokeVoidAsync("console.log", $"{first_login_currency_type_value}");

                                    float? user_convert_av_balance = ConvertCurrency(requiredDetails.CurrencyTypeUser, first_login_currency_type_value, user_data.UserAvailableBalance);
                                    float? user_convert_debt_balance = ConvertCurrency(requiredDetails.CurrencyTypeUser, first_login_currency_type_value, user_data.UserDebtBalance);

                                    await JS.InvokeVoidAsync("console.log", "debt balance value after concersion");
                                    await JS.InvokeVoidAsync("console.log", $"{user_convert_debt_balance}");

                                    await JS.InvokeVoidAsync("console.log", "available balance value after concersion");
                                    await JS.InvokeVoidAsync("console.log", $"{user_convert_av_balance}");


                                    user_data.UserDebtBalance = ConvertCurrency(requiredDetails.CurrencyTypeUser, first_login_currency_type_value, user_data.UserDebtBalance);
                                    user_data.UserAvailableBalance = ConvertCurrency(requiredDetails.CurrencyTypeUser, first_login_currency_type_value, user_data.UserAvailableBalance);

                                    await JS.InvokeVoidAsync("console.log", "debt balance value after concersion");
                                    await JS.InvokeVoidAsync("console.log", $"{user_data.UserAvailableBalance}");

                                    await JS.InvokeVoidAsync("console.log", "debt balance value after concersion");
                                    await JS.InvokeVoidAsync("console.log", $"{user_data.UserDebtBalance}");

                                    await JS.InvokeVoidAsync("showAlert", "Login Success.Welcome to user home page");
                                    Navigation.NavigateTo("/userhome");
                                    return;
                                }
                                if (already_login == false)
                                {
                                    await JS.InvokeVoidAsync("console.log", "first login value");
                                    await JS.InvokeVoidAsync("console.log", $"{requiredDetails.FirstLoginCurrencyType.Count()}");

                                    requiredDetails.FirstLoginCurrencyType.Add(first_login_currency_type);

                                    await JS.InvokeVoidAsync("console.log", "first login value");
                                    await JS.InvokeVoidAsync("console.log", $"{requiredDetails.FirstLoginCurrencyType.Count()}");
                                    await JS.InvokeVoidAsync("console.log", $"{requiredDetails.FirstLoginCurrencyType[0]["firstlogin_currency_type"]}");
                                    await JS.InvokeVoidAsync("console.log", "User currency type.");
                                    await JS.InvokeVoidAsync("console.log", $"{requiredDetails.CurrencyTypeUser.ToString()}");
                                    await JS.InvokeVoidAsync("console.log", "login success");
                                    await JS.InvokeVoidAsync("showAlert", "Login Success.Welcome to user home page");
                                    Navigation.NavigateTo("/userhome");
                                    return;
                                }
                            }
                            else
                            {
                                await JS.InvokeVoidAsync("console.log", "first login value");
                                await JS.InvokeVoidAsync("console.log", $"{requiredDetails.FirstLoginCurrencyType.Count()}");

                                requiredDetails.FirstLoginCurrencyType.Add(first_login_currency_type);

                                await JS.InvokeVoidAsync("console.log", "first login value");
                                await JS.InvokeVoidAsync("console.log", $"{requiredDetails.FirstLoginCurrencyType.Count()}");
                                await JS.InvokeVoidAsync("console.log", $"{requiredDetails.FirstLoginCurrencyType[0]["firstlogin_currency_type"]}");

                                await JS.InvokeVoidAsync("console.log", "User currency type.");
                                await JS.InvokeVoidAsync("console.log", $"{requiredDetails.CurrencyTypeUser.ToString()}");
                                await JS.InvokeVoidAsync("console.log", "login success");
                                await JS.InvokeVoidAsync("showAlert", "Login Success.Welcome to user home page");
                                Navigation.NavigateTo("/userhome");
                                return;
                            }

                        }

                        if (user_details.UserType == "admin")
                        {
                            requiredDetails.CurrencyTypeUser = currencyType;
                            await JS.InvokeVoidAsync("console.log", "User currency type.");
                            await JS.InvokeVoidAsync("console.log", $"{requiredDetails.CurrencyTypeUser.ToString()}");
                            await JS.InvokeVoidAsync("console.log", "login success");
                            requiredDetails.CurrentUserUsername = username;
                            await JS.InvokeVoidAsync("showAlert", "Login Success.Welcome to admin home page");
                            Navigation.NavigateTo("/adminhome");
                            return;
                        }
                    }
                    else
                    {
                        await JS.InvokeVoidAsync("showAlert", "Password incorrect.");
                        return;
                    }
                }
                else
                {
                    await JS.InvokeVoidAsync("showAlert", "User don't found.Sign up.");
                    return;
                }
            }
            catch (Exception ex)
            {
                await JS.InvokeVoidAsync("console.log", $"Exception caught at login method.Exception = {ex.ToString()}");
            }

        }




        public bool CheckUserFirstLogin(
        List<Dictionary<string, string>> listOfDictionaries,
        string keyToFind,
        string valueToMatch
        )
        {
            foreach (var dictionary in listOfDictionaries)
            {
                if (dictionary.ContainsKey(keyToFind) && dictionary[keyToFind] == valueToMatch)
                {
                    return true;
                    break;
                }
            }
            return false;
        }

        private Dictionary<string, float> ExchangeRatesToUSD = new()
    {
        { "USD", 1.0f },
        { "NRS", 0.0076f },
        { "BGP", 1.26f },
        { "INRS", 0.012f }
    };

        // Convert the amount from one currency to another

        public float ConvertCurrency(string currentCurrency, string firstLoginCurrency, float amount)
        {
            try
            {
                if (firstLoginCurrency == null || String.IsNullOrEmpty(firstLoginCurrency))
                {
                    return amount;
                }
                else
                {
                    // Get exchange rates
                    float currentToUSD = ExchangeRatesToUSD[currentCurrency];
                    float firstLoginToUSD = ExchangeRatesToUSD[firstLoginCurrency];

                    // Convert amount to USD
                    float amountInUSD = amount * firstLoginToUSD;

                    // Convert USD amount to target currency
                    float convertedAmount = amountInUSD / currentToUSD;

                    return convertedAmount;
                }

            }
            catch (Exception obj)
            {
                return amount;
            }
        }


        public string GetFirstLoginCurrencyType(string username)
        {
            try
            {

                if (requiredDetails.FirstLoginCurrencyType.Count() >= 1)
                {
                    foreach (var userDetails in requiredDetails.FirstLoginCurrencyType)
                    {
                        if (userDetails.ContainsKey("user_username") && userDetails["user_username"] == requiredDetails.CurrentUserUsername)
                        {

                            return userDetails["firstlogin_currency_type"];
                        }
                    }
                }
                else
                {
                    return null;
                }
                return null;
            }
            catch (Exception obj)
            {
                return null;
            }
        }


    }
}

