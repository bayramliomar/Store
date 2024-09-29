using OtpNet;
using QRCoder;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;

namespace Store.Services
{
    public class GoogleAuthenticatorService
    {
        // Generate a random secret key for the user
        public string GenerateSecretKey()
        {
            var key = KeyGeneration.GenerateRandomKey(20);
            return Base32Encoding.ToString(key);
        }

        // Generate a QR code for Google Authenticator
        public string GenerateQrCodeUri(string userName, string secretKey, string issuer)
        {
            string setupInfo = $"otpauth://totp/{HttpUtility.UrlEncode(userName)}?secret={secretKey}&issuer={issuer}";
            return setupInfo;
        }

        // Generate QR code image as base64 string
        public string GenerateQrCodeImage(string setupUri)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(setupUri, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);

            using (MemoryStream ms = new MemoryStream())
            {
                qrCodeImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                byte[] byteImage = ms.ToArray();
                return "data:image/png;base64," + Convert.ToBase64String(byteImage);
            }
        }

        // Verify the TOTP code from Google Authenticator
        public bool VerifyCode(string secretKey, string code)
        {
            var key = Base32Encoding.ToBytes(secretKey);
            var totp = new Totp(key);
            return totp.VerifyTotp(code, out long timeStepMatched, new VerificationWindow(2, 2));
        }
    }
}