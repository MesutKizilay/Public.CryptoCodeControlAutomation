using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCodeControlAutomation.Application.Features.Auth.Constants
{
    public static class AuthMessages
    {
        public const string UserDoesntExist = "Kullanıcı sisteme kayıtlı değil.";
        public const string PasswordDoesntMatch = "Kullanıcı şifresi hatalı.";
        public const string UserIsNotActive = "Kullanıcınız pasif durumdadır.";
        public const string OperatorRoleShouldBeExist = "Operator rolü yetkili tarafından veri tabanına kaydedilmemiş.";
    }
}