/*
The MIT License (MIT)

Copyright (c) 2018 Giovanni Paolo Vigano'

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using M2MqttUnity;
using Unity.VisualScripting;
using Hyperledger.Indy.WalletApi;
using Hyperledger.Indy.DidApi;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Parameters;
using System.IO;
using Org.BouncyCastle.Bcpg.OpenPgp;

/// <summary>
/// Examples for the M2MQTT library (https://github.com/eclipse/paho.mqtt.m2mqtt),
/// </summary>
namespace M2MqttUnity.Examples
{
    /// <summary>
    /// Script for testing M2MQTT with a Unity UI
    /// </summary>
    public class M2MqttUnityTest : M2MqttUnityClient
    {
        private List<string> eventMessages = new List<string>();
        private bool updateUI = false;
        private DidUser didUser;
        private HttpClient httpClient;
        private DidSystem didSystem;

        [Tooltip("Set this to true to perform a testing cycle automatically on startup")]
        public bool autoTest = false;
        [Header("User Interface")]
        public InputField consoleInputField;
        public Toggle encryptedToggle;
        public InputField addressInputField;
        public InputField portInputField;
        public InputField messageInputField;
        public Button connectButton;
        public Button disconnectButton;
        public Button testPublishButton;
        public Button clearButton;
        public Button sendButton;

        //string publicKey, privateKey;

        private string privateKey =
@"-----BEGIN RSA PRIVATE KEY-----
MIIEvAIBADANBgkqhkiG9w0BAQEFAASCBKYwggSiAgEAAoIBAQCvUmQXXc30gdks
sUxiDe1lrR97BMAeVBzrSm61Lj3mmiOYJ+N/BhpVZa3K3AORscOq59ikds3hJd6U
o7i4XLy0yqBLjw+oKg7vp+DGHhUXAepEUu3RxXvfPiShh8U1s4sfgTNun+sl5A69
M1NaVfMMDts3Qfc8orM6jvfxlqNFeVqWVyUQjiJ1E5fqnCiV5WLWV/7NuWxEIVhp
0gmI3QNvpU9aF4mutbAmgtn21wFnT4i+EZ4eLwueSUEmMSRcuxykhqBgl1P1GKH5
N/i3aClOwVaZX7fme4SWIipwWL/WGl/kw/QtAFud56cFifOekG5t2QZdWYdGuTbj
hilx0eU1AgMBAAECggEAK5d5yZGKTvwmKoWe71Z1K23DQQwqVKAnXJ1yL1xjCb5T
kVaBqyiyTX9tmd3giU5Az3K8HBBqGkwXLrASksyEIxOqjX7xrqdedVoxejEqn0Db
pcdU2G6Y4SYLZ5phf3u34Mp4Jk5e4ln1DDgBxplOSDY9bVzag1oU5D9+CLSivfgH
ykDAgV68bZ8WpyuoI4JixChz2rvmWGDu0qjqHnTwV0r08MUMEblswco1Q1ZVcqVz
mRc065E5xrZS7w/49f7E+oVrmE+hG3TC7why+eaNnlcGACxDnOCfZs0vCGzkS7iC
42+5NR16co39gP9B9hAy8V2zXLxrD01KWV1ToBT8dQKBgQDlv+AHejWLlMfFEMLx
QGHp9MRQ+62ePAuKeFv3bT5u1+/rCAxJ4Ufg5oNU7g7rmdyPrnNFR6bmhcasIo8M
fA3DMdGfYg0TVpqsOjVYOc/Q2zRpByYvDjRUfmxSzexP/Rl1iin+R2xukVhP+H4Z
OmBIB5HuXSS9xsGR6J6TZ+xBywKBgQDDWoSVSE8afeTqpee+TIjjT2eIYTvom+5K
L93nYrk6dXCLpdPPWfdEeiiJ3VFBai1TOtc75zK88+9RpGYIQxdI3n1LDpuzNYB7
shsmjqHrsLtw5A5Fih2NE1FknAfZFOT5D0K+CInMt3dBUuHsP7zR/qRm2Bnk6zdj
BI5N6/mU/wKBgHkYdE2cpYJnrg/5sLaWN8WnxJ3mufEwNukKXXcBtqmX2ZYpDMkY
G9m3xjtKqsSVuYJl8c3vYVIZ6siqAnoTHPHoXVPDy56IxEfI+nsBJb8w+uPs57xf
oUdzx0ax0T+r6PJiG5YyMT3qEAE+ucA0W7E7hDh+EbKRg0+Tq5mIaieXAoGAWZXb
EzJzxCxTnB+05JnodIVnby1X8dYAMtk8o+2sD9jnYcMMHRyevjJfAMoThzIP0wk6
xufBZtFewEvp1oQd23bccl0inc49O3xz4vyp2JHVg3Gx8cXiw11GiCLdnnlsf6K1
8rurpcvEzpoZrOOVzZ8++ULErSwI3EmskKNUczUCgYBIRoMcqpdHnc3j6Hs7P3uv
Pd9B2MjQUMYiMUHmKTc8cGRDThNA4o8vW9yySwtPUBA04SK5PuwxaB2Bq1eXrjH9
Luc3FvO+yFXc9m8OG3grsQfyfKIqvs3N3kFX5+uJG2QSZHEHrLoM2LTrQSnFs20D
G6diGqRC5ni";
        private string publicKey = @"-----BEGIN PUBLIC KEY-----
MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAr1JkF13N9IHZLLFMYg3t
Za0fewTAHlQc60putS495pojmCfjfwYaVWWtytwDkbHDqufYpHbN4SXelKO4uFy8
tMqgS48PqCoO76fgxh4VFwHqRFLt0cV73z4koYfFNbOLH4Ezbp/rJeQOvTNTWlXz
DA7bN0H3PKKzOo738ZajRXlallclEI4idROX6pwoleVi1lf+zblsRCFYadIJiN0D
b6VPWheJrrWwJoLZ9tcBZ0+IvhGeHi8LnklBJjEkXLscpIagYJdT9Rih+Tf4t2gp
TsFWmV+35nuEliIqcFi/1hpf5MP0LQBbneenBYnznpBubdkGXVmHRrk244YpcdHl
NQIDAQAB
-----END PUBLIC KEY-----
";
        
        [Header("MQTT Settings")]
        [Tooltip("Topic to subscribe to")]
        public List<string> topic;
        public string targetDid;

        protected override void Start()
        {
            SetUiMessage("Ready.");
            updateUI = true;
            base.Start();

            
            //GenerateKeyPair(out publicKey, out privateKey);

            Debug.Log("Public Key: " + publicKey);
            Debug.Log("Private Key: " + privateKey);

            didUser = this.GetComponent<DidUser>();
            httpClient = this.GetComponent<HttpClient>();
            didSystem = this.GetComponent<DidSystem>();
        }

        public void TestPublish()
        {
            foreach (string i in topic)
            {
                client.Publish(i, System.Text.Encoding.UTF8.GetBytes("Test message"), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
                Debug.Log("Test message published");
                AddUiMessage("Test message published.");
            }
        }

        public void SetBrokerAddress(string brokerAddress)
        {
            if (addressInputField && !updateUI)
            {
                this.brokerAddress = brokerAddress;
            }
        }

        public void SetBrokerPort(string brokerPort)
        {
            if (portInputField && !updateUI)
            {
                int.TryParse(brokerPort, out this.brokerPort);
            }
        }


        public void SendMessage2()
        {
            if (messageInputField)
            {
                string message = messageInputField.text;

                //string signedMessage = didUser.PackMessage(message, targetDid);

                if (message != "")
                {
                    foreach (string i in topic)
                    {
                        //client.Publish(i, System.Text.Encoding.UTF8.GetBytes(message), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);

                        PublishEncryptedMessage(i, message);
                        Debug.Log("Message published");
                        AddUiMessage("Message published.");
                    }
                }
            }
        }

        protected override void OnConnected()
        {
            base.OnConnected();
            SetUiMessage("Connected to broker on " + brokerAddress + "\n");

            //didUser.StartDPKI(didSystem, httpClient);

            if (autoTest)
            {
                TestPublish();
            }


        }

        public void SetEncrypted(bool isEncrypted)
        {
            this.isEncrypted = isEncrypted;
            Debug.Log("isEncrypted: " + isEncrypted);
        }


        public void SetUiMessage(string msg)
        {
            if (consoleInputField != null)
            {
                consoleInputField.text = msg;
                updateUI = true;
            }
        }

        public void AddUiMessage(string msg)
        {
            if (consoleInputField != null)
            {
                consoleInputField.text += msg + "\n";
                updateUI = true;
            }
        }

        protected override void OnConnecting()
        {
            base.OnConnecting();
            SetUiMessage("Connecting to broker on " + brokerAddress + ":" + brokerPort.ToString() + "...\n");
        }

        protected override void SubscribeTopics()
        {
            foreach (string i in topic)
            {
                client.Subscribe(new string[] { i }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
            }
            //client.Subscribe(new string[] { topic }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
        }

        protected override void UnsubscribeTopics()
        {
            foreach (string i in topic)
            {
                client.Unsubscribe(new string[] { i });
            }
            //client.Unsubscribe(new string[] { topic });
        }

        protected override void OnConnectionFailed(string errorMessage)
        {
            AddUiMessage("CONNECTION FAILED! " + errorMessage);
        }

        protected override void OnDisconnected()
        {
            AddUiMessage("Disconnected.");
        }

        protected override void OnConnectionLost()
        {
            AddUiMessage("CONNECTION LOST!");
        }

        private void UpdateUI()
        {
            if (client == null)
            {
                if (connectButton != null)
                {
                    connectButton.interactable = true;
                    disconnectButton.interactable = false;
                    testPublishButton.interactable = false;
                }
            }
            else
            {
                if (testPublishButton != null)
                {
                    testPublishButton.interactable = client.IsConnected;
                }
                if (disconnectButton != null)
                {
                    disconnectButton.interactable = client.IsConnected;
                }
                if (connectButton != null)
                {
                    connectButton.interactable = !client.IsConnected;
                }
            }
            if (addressInputField != null && connectButton != null)
            {
                addressInputField.interactable = connectButton.interactable;
                addressInputField.text = brokerAddress;
            }
            if (portInputField != null && connectButton != null)
            {
                portInputField.interactable = connectButton.interactable;
                portInputField.text = brokerPort.ToString();
            }
            if (encryptedToggle != null && connectButton != null)
            {
                encryptedToggle.interactable = connectButton.interactable;
                encryptedToggle.isOn = isEncrypted;
            }
            if (clearButton != null && connectButton != null)
            {
                clearButton.interactable = connectButton.interactable;
            }
            updateUI = false;
        }

        protected override void DecodeMessage(string topic, byte[] message)
        {
            /*
            string msg = System.Text.Encoding.UTF8.GetString(message);
            Debug.Log("Received: " + msg);
            StoreMessage(msg);

            foreach (string i in this.topic)
            {
                if (topic == i)
                {
                    if (autoTest)
                    {
                        autoTest = false;
                        Disconnect();
                    }
                }
            }
             */
            
            string encryptedMsg = Encoding.UTF8.GetString(message);
            Debug.Log("Received encrypted message: " + encryptedMsg);

            byte[] decryptedMsgBytes = DecryptString(encryptedMsg, privateKey);
            string decryptedMsg = Encoding.UTF8.GetString(decryptedMsgBytes);

            Debug.Log("Decrypted message: " + decryptedMsg);

            base.DecodeMessage(topic, Encoding.UTF8.GetBytes(decryptedMsg));
            
        }

        private void StoreMessage(string eventMsg)
        {
            eventMessages.Add(eventMsg);
        }

        private void ProcessMessage(string msg)
        {
            AddUiMessage("Received: " + msg);
        }

        protected override void Update()
        {
            base.Update(); // call ProcessMqttEvents()

            if (eventMessages.Count > 0)
            {
                foreach (string msg in eventMessages)
                {
                    ProcessMessage(msg);
                }
                eventMessages.Clear();
            }
            if (updateUI)
            {
                UpdateUI();
            }
        }



        private void OnDestroy()
        {
            Disconnect();
        }

        private void OnValidate()
        {
            if (autoTest)
            {
                autoConnect = true;
            }
        }

        public string EncryptString(string plainText, string publicKey)
        {
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                try
                {
                    rsa.FromXmlString(publicKey.ToString());
                    var encryptedData = rsa.Encrypt(Encoding.UTF8.GetBytes(plainText), true);
                    var base64Encrypted = Convert.ToBase64String(encryptedData);
                    return base64Encrypted;
                }
                finally
                {
                    rsa.PersistKeyInCsp = false; // Set this to false to avoid permission issues.
                }
            }


        }

        public void PublishEncryptedMessage(string topic, string message)
        {
            // Replace 'publicKey' with the actual public key of the receiver.
            
            /*
             * client.Publish(topic,
                           Encoding.UTF8.GetBytes(EncryptString(message, PublicKey)),
                           MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE,
                           false);
            */
        }

        public byte[] DecryptString(string cipherText, string pemPrivateKey)
        {
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                try
                {
                    string xmlPrivateKey = ConvertPemToXml(pemPrivateKey);
                    rsa.FromXmlString(xmlPrivateKey);

                    var base64Encrypted = Convert.FromBase64String(cipherText);
                    var decryptedData = rsa.Decrypt(base64Encrypted, false); // Use 'false' for PKCS#1 v1.5 padding.

                    return decryptedData;
                }
                finally
                {
                    rsa.PersistKeyInCsp = false;
                }
            }
        }

        public void GenerateKeyPair(out string publicKey, out string privateKey)
        {
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                rsa.PersistKeyInCsp = false; // Don't store the keys in a key container
                publicKey = rsa.ToXmlString(false); // False to get the public key
                privateKey = rsa.ToXmlString(true); // True to get the private key
            }
        }

        private string ConvertPemToXml(string pemPrivateKey)
        {
            using (var textReader = new StringReader(pemPrivateKey))
            {
                var pemReader = new PemReader(textReader);
                var keyPair = (AsymmetricCipherKeyPair)pemReader.ReadObject();
                var rsaParams = DotNetUtilities.ToRSAParameters((RsaPrivateCrtKeyParameters)keyPair.Private);

                var rsaProvider = new RSACryptoServiceProvider();
                rsaProvider.ImportParameters(rsaParams);

                return rsaProvider.ToXmlString(true);
            }
        }
    }
}

