using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace ActivityJournal.Models
{
    /**
     * Class used for registration purposes 
     */
    public class UserRegister
    {
        public string Firstname { get; set; } = string.Empty;
        public string Lastname { get; set; } = string.Empty;

        [DataType(DataType.EmailAddress)]
        public string Email { get; set; } = string.Empty;

        [DataType(DataType.Password)]

        public string Password { get; set; } = string.Empty;
        [DataType(DataType.Password)]

        public string ConfirmPassword { get; set; } = string.Empty;

        /**
         * Creates hashed password and salt that then is stored in the database
         */
        public void createHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

    }
    /**
     * Used for login purposes
     */
    public class UserLogin
    {
        public string Email { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool VerifyHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computeHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computeHash.SequenceEqual(passwordHash);
            }
        }
    }
}
