using Google.Cloud.Kms.V1;
using Google.Protobuf;
using Grpc.Auth;
using System;

namespace BlogCore.Models.Services
{
    public class KmsService
    {
        private readonly KeyManagementServiceClient _client;
        private readonly CryptoKeyName _keyName;

        public KmsService(string credentialsPath)
        {
            var builder = new KeyManagementServiceClientBuilder
            {
                CredentialsPath = credentialsPath
            };

            _client = builder.Build();

            // TODO: valores reales
            _keyName = new CryptoKeyName("dark-star-465316-e8", "global", "blogcore-keyring", "articulo-key");
        }

        public string Cifrar(string textoPlano)
        {
            var plaintextBytes = ByteString.CopyFromUtf8(textoPlano);
            var respuesta = _client.Encrypt(_keyName, plaintextBytes);
            return Convert.ToBase64String(respuesta.Ciphertext.ToByteArray());
        }

        public string Descifrar(string textoCifradoBase64)
        {
            var ciphertextBytes = ByteString.CopyFrom(Convert.FromBase64String(textoCifradoBase64));
            var respuesta = _client.Decrypt(_keyName, ciphertextBytes);
            return respuesta.Plaintext.ToStringUtf8();
        }
    }
}
