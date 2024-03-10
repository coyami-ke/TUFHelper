using UnityEngine.Networking;

namespace Together.Utils
{
    public class CertificateWhore : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            return true;
        }

    }
}

