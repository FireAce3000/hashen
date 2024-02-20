using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System;
using Bsp1.Models;

namespace Bsp1
{
    internal class Program
    {
        // Const for the name and password length 
        const int NAME_MIN = 3;
        const int NAME_MAX = 16;
        const int PASSWORD_MIN = 3;
        const int PASSWORD_MAX = 16;

        static void Main(string[] args)
        {
            // Date
            DateTime today = DateTime.Now;

            // Headline
            Console.WriteLine($"--- {today.ToString("yyyy_MM_dd")}_Login_Hash ---");

            Person personReg = new Person(){Name= "", Password = ""};
            Person personLogin = new Person();
            int selection = 0;

            // Evertime use here the same Salt...
            byte[] saltArray = GetSalt();

            do
            {
                // Selection
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("[1] to regist [2] login [3] exit : ");
                try
                {
                    selection = int.Parse(Console.ReadLine());
                    Console.ForegroundColor = ConsoleColor.White;

                    switch (selection)
                    {
                        case 1:
                            Console.WriteLine("--- TO REGISTER ---");

                            Console.Write($"Name (min {NAME_MIN}/max {NAME_MAX}): ");
                            personReg.Name = Console.ReadLine();

                            Console.Write($"Password (min {PASSWORD_MIN}/max {PASSWORD_MAX}): ");
                            // string pwReg = Console.ReadLine();
                            personReg.Password = ReadPasswordFromConsole();

                            // check with status name and password shorter than X and longer than X (CONST)
                            Status statusReg = CheckLenght(personReg.Name, personReg.Password);

                            // Coloring the output
                            if (statusReg.statusBool) Console.ForegroundColor = ConsoleColor.Green;
                            else Console.ForegroundColor = ConsoleColor.Red;

                            Console.Write(statusReg.statusString);
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write(" / ");

                            // How strong is the password with coloring
                            string strong = IsStrongPassword(personReg.Password);
                            if (strong.Contains("NOT")) Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine(strong);
                            Console.ForegroundColor = ConsoleColor.White;

                            // Password hashen
                            personReg.Hash = CreateSha256Hash(personReg.Password, saltArray);
                            break;

                        case 2:
                            Console.WriteLine();
                            Console.WriteLine("--- LOGIN ---");

                            Console.Write($"Name (min {NAME_MIN}/max {NAME_MAX}): ");
                            personLogin.Name = Console.ReadLine();

                            Console.Write($"Password (min {PASSWORD_MIN}/max {PASSWORD_MAX}): ");
                            // string pwLogin = Console.ReadLine();
                            personLogin.Password = ReadPasswordFromConsole();

                            // Password hashen
                            string pwLoginHash = CreateSha256Hash(personLogin.Password, saltArray);

                            // Check
                            if (personReg.Name.Equals(personLogin.Name) && personReg.Hash.Equals(personLogin.Hash))
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("--- CONGRATULATIONS! You are logged in ---");
                                Console.ForegroundColor = ConsoleColor.White;

                                // Info
                                Console.WriteLine(@$"
> INFO:
> Name: {personReg.Name}
> Password: {personReg.Password}
> Hash: {personReg.Hash}
");

                                // Logout and close
                                Console.Write("Enter to logout");
                                Console.Read();
                                Console.WriteLine("--- LOGOUT ---");
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("--- MISTAKE! Name or password are not correct ---");
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                            break;
                        case 3:
                            Console.WriteLine("--- EXIT ---");
                            break;
                        default:
                            Console.WriteLine("Input isnt correct");
                            break;
                    }
                }
                catch
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Input isnt correct");
                }
            } while (selection != 3);
        }

        /// <summary>
        /// Dont display the entered character
        /// </summary>
        /// <returns>password</returns>
        public static string ReadPasswordFromConsole()
        {
            string password = "";
            ConsoleKeyInfo key;

            do
            {
                // save the chars, but dont see it in the console
                key = Console.ReadKey(true);

                // When Enter or Backspace
                if (key.Key != ConsoleKey.Enter && key.Key != ConsoleKey.Backspace)
                {
                    // Add the Input Char
                    password += key.KeyChar;
                }
                // When Backspace, delete the char
                else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    // Delete the last Char
                    password = password.Remove(password.Length - 1);
                }
            } while (key.Key != ConsoleKey.Enter);

            // next line
            Console.WriteLine();

            return password;
        }

        /// <summary>
        /// Generate a random salt array
        /// </summary>
        /// <returns>saltArray byte[]</returns>
        private static byte[] GetSalt()
        {
            // Using RandomNumberGenerator (obsulete RNGCryptoServiceProvider)
            using (var rng = RandomNumberGenerator.Create())
            {
                // Maximum length of salt
                int maxLength = 32;

                // Empty salt array
                byte[] saltArray = new byte[maxLength];

                // Build the random bytes
                rng.GetBytes(saltArray);

                // Return array (for check) encoded salt
                return saltArray;
            }
        }

        /// <summary>
        /// Create a SHA256-Hashvalue (password and salt)
        /// </summary>
        /// <param name="password"></param>
        /// <param name="saltArray"></param>
        /// <returns>shaString</returns>
        private static string CreateSha256Hash(string password, byte[] saltArray)
        {
            // Combining password and salt
            byte[] combinedBytes = Encoding.UTF8.GetBytes(password).Concat(saltArray).ToArray();

            // Create a SHA256
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array
                byte[] hashBytes = sha256Hash.ComputeHash(combinedBytes);

                // Convert byte array to a string
                StringBuilder builder = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                string hashString = builder.ToString();

                return hashString;
            }
        }

        /// <summary>
        /// Status for the return in CheckLenght()
        /// </summary>
        private struct Status
        {
            public bool statusBool;
            public string statusString;
        }

        /// <summary>
        /// Check name and password to register
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pw"></param>
        /// <returns>Struct status with bool and string</returns>
        private static Status CheckLenght(string name, string pw)
        {
            // Return value
            Status status;

            // Check null or empty
            if (String.IsNullOrEmpty(name) || String.IsNullOrEmpty(pw))
            {
                status.statusString = "MISTAKE! Name or Password is null or empty!";
                status.statusBool = false;
                return status;
            }

            // Check lenght (CONST)
            if (name.Length >= NAME_MIN && pw.Length >= PASSWORD_MIN && name.Length <= NAME_MAX && pw.Length <= PASSWORD_MAX)
            {
                status.statusString = "Name and password are correct";
                status.statusBool = true;
                return status;
            }
            else if (name.Length < NAME_MIN)
            {
                status.statusString = $"MISTAKE! Name is shorter than {NAME_MIN} chars!";
                status.statusBool = false;
                return status;
            }
            else if (name.Length > NAME_MAX)
            {
                status.statusString = $"MISTAKE! Name is longer than {NAME_MAX} chars!";
                status.statusBool = false;
                return status;
            }
            else if (pw.Length < PASSWORD_MIN)
            {
                status.statusString = $"MISTAKE! Password is shorter than {PASSWORD_MIN} chars!";
                status.statusBool = false;
                return status;
            }
            else if (pw.Length > PASSWORD_MAX)
            {
                status.statusString = $"MISTAKE! Password is longer than {PASSWORD_MAX} chars!";
                status.statusBool = false;
                return status;
            }
            else
            {
                status.statusString = "DEFAULT!";
                status.statusBool = false;
                return status;
            }
        }

        /// <summary>
        /// Check for strong password
        /// </summary>
        /// <param name="password"></param>
        /// <returns>isStrongPW</returns>
        private static string IsStrongPassword(string password)
        {
            // vars for checking
            bool result;
            string specialCharacters = @"!@#$%^&*()-_=+[]{}|;:'"",.<>/?";
            string isStrongPW;

            // Check for Upperchars
            if (!password.Any(char.IsUpper))
            {
                result = false;
            }
            // Check for Lowerchars
            else if (!password.Any(char.IsLower))
            {
                result = false;
            }
            // Check for numbers
            else if (!password.Any(char.IsDigit))
            {
                result = false;
            }
            // check the specialChars
            else if (!password.Any(specialCharacters.Contains))
            {
                result = false;
            }
            // password is strong
            else
            {
                result = true;
            }

            // return
            if (result) isStrongPW = "Password is strong!";
            else isStrongPW = $"Password is NOT strong!";

            return isStrongPW;
        }
    }
}

