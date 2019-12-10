using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Security;

namespace BaumRoll40.Models
{
    public class CustomMembershipProvider : MembershipProvider
    {
        /// <summary>
        /// ストレッチング回数
        /// </summary>
        const int STRETCHING_TIMES = 10000;

        public override bool EnablePasswordRetrieval => throw new NotImplementedException();

        public override bool EnablePasswordReset => throw new NotImplementedException();

        public override bool RequiresQuestionAndAnswer => throw new NotImplementedException();

        public override string ApplicationName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override int MaxInvalidPasswordAttempts => throw new NotImplementedException();

        public override int PasswordAttemptWindow => throw new NotImplementedException();

        public override bool RequiresUniqueEmail => throw new NotImplementedException();

        public override MembershipPasswordFormat PasswordFormat => throw new NotImplementedException();

        public override int MinRequiredPasswordLength => throw new NotImplementedException();

        public override int MinRequiredNonAlphanumericCharacters => throw new NotImplementedException();

        public override string PasswordStrengthRegularExpression => throw new NotImplementedException();

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            throw new NotImplementedException();
        }

        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            throw new NotImplementedException();
        }

        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override int GetNumberOfUsersOnline()
        {
            throw new NotImplementedException();
        }

        public override string GetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            throw new NotImplementedException();
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            throw new NotImplementedException();
        }

        public override string GetUserNameByEmail(string email)
        {
            throw new NotImplementedException();
        }

        public override string ResetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }

        public override bool UnlockUser(string userName)
        {
            throw new NotImplementedException();
        }

        public override void UpdateUser(MembershipUser user)
        {
            throw new NotImplementedException();
        }

        public override bool ValidateUser(string userId, string password)
        {
            if(userId == null)
            {
                return false;
            }

            using (var db = new BaumRollEntities())
            {
                string passhash = GeneratePasswordHash(userId, password);

                var id = int.Parse(userId);
                var user = db.Users
                    .Where(u => u.UserId.Equals(id) && u.Password.Equals(passhash))
                    .FirstOrDefault();

                if (user != null)
                {
                    // 認証OK
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// UserNameからsaltを取得
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        private string GenerateSalt(string username)
        {
            // 文字列をbyte配列に変換
            var data = System.Text.Encoding.UTF8.GetBytes(username);
            // SHA256CryptoServiceProvider
            var sha256 = new SHA256CryptoServiceProvider();
            // ハッシュ値を計算する
            var hash = sha256.ComputeHash(data);
            // 文字列に変換
            string result = BitConverter.ToString(hash).ToLower().Replace("-", "");

            return result;
        }

        /// <summary>
        /// ユーザー名とパスワードからパスワードハッシュを取得
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public string GeneratePasswordHash(string username, string password)
        {
            // saltを取得
            string salt = this.GenerateSalt(username);

            // PBKDF2でパスワードをハッシュ化
            var pbkdf2 = new Rfc2898DeriveBytes(password,
                System.Text.Encoding.UTF8.GetBytes(salt),
                STRETCHING_TIMES);

            // パスワードハッシュ(byte)をbase64形式の文字列に変換
            string result = Convert.ToBase64String(pbkdf2.GetBytes(32));

            return result;
        }
    }
}