using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Hyperledger.Indy.WalletApi;
using Hyperledger.Indy.DidApi;
using Hyperledger.Indy.PoolApi;
using Hyperledger.Indy.LedgerApi;
using Hyperledger.Indy.CryptoApi;
using Hyperledger.Indy.AnonCredsApi;
using System.Text;
using Newtonsoft.Json.Linq;
using System;

public class DidUser : MonoBehaviour
{
    Wallet wallet;

    string walletName = "wallet";
    string walletKey = "walletCredentials";

    public Dictionary<string, string> didVerKeyMap = new Dictionary<string, string>();

    public string targetVk = "sdasdadinadowindoianwido123";//test

    public CreateAndStoreMyDidResult didAndVerkey;
    public string userDid = "userDid00000000000000000000000000";
    public string targetDid = "targetDid00000000000000000000000000";

    string walletConfig;
    string walletCredentials;

    void Start()
    {

    }

    public void StartDPKI(DidSystem didSystem, HttpClient httpclient)
    {
        // Get instance of DidSystem
        didSystem = DidSystem.GetInstance();

        // Create and open a new wallet using the methods from DidSystem
        this.wallet = didSystem.CreateWalletTest();

        // Create and store a new DID using the methods from DidSystem
        this.didAndVerkey = didSystem.CreateDidInWalletTest(wallet, userDid);

        WalletCreateAndOpen();
        CreateAndStoreDid();


        CreateAndStoreMyDidResult didAndVerkey = didSystem.CreateDidInWalletTest(wallet, userDid);

        didSystem.StartDidSyetem(didSystem, httpclient, userDid, targetDid);
    }

    void WalletCreateAndOpen()
    {
        Debug.Log("DID User Create Wallet");
        string walletTestName = this.walletName + UnityEngine.Random.Range(0, 1000).ToString();
        walletConfig = "{\"id\":\"" + walletTestName + "\"}";
        //string walletConfig = "{\"id\":\"" + this.walletName + "\"}";
        //walletConfig = "{\"id\":\"" + walletName + "\", \"storageConfig\": {\"path\": \"" 
        //    + defultWalletPath + "\"}}";
        walletCredentials = "{\"key\":\"" + this.walletKey + "\"}";

        Wallet.CreateWalletAsync(walletConfig, walletCredentials).Wait();
        wallet = Wallet.OpenWalletAsync(walletConfig, walletCredentials).Result;
    }

    void CreateAndStoreDid()
    {
        Debug.Log("CreateAndStoreDid");
        string didConfig = "{\"seed\":\"" + userDid + "\"}";
        didAndVerkey = Did.CreateAndStoreMyDidAsync(wallet, didConfig).Result;
        Debug.Log("didAndVerkey: " + didAndVerkey.Did + " " + didAndVerkey.VerKey);

        // Store the DID and its corresponding VerKey in the dictionary.
        if (!didVerKeyMap.ContainsKey(didAndVerkey.Did))
            didVerKeyMap.Add(didAndVerkey.Did, didAndVerkey.VerKey);
    }

    public void CleanWallet()
    {
        if (wallet == null)
        {
            Debug.Log("this.wallet is null");
        }

        Debug.Log(didAndVerkey.Did + " - CleanWallet");
        wallet.CloseAsync().Wait();
        Wallet.DeleteWalletAsync(walletConfig, walletCredentials).Wait();
    }

    public string PackMessage(string message, string targetDid)
    {
        Debug.Log("PackMessage : " + message);

        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(message);

        Debug.Log("bytes: " + BitConverter.ToString(bytes));

        byte[] packedMessage = Crypto.PackMessageAsync(wallet, didAndVerkey.VerKey,
            targetDid, bytes).Result;

        Debug.Log("packedMessage: " + packedMessage.ToString());

        return Encoding.UTF8.GetString(packedMessage);
    }

    public string PackMessage(string message, CreateAndStoreMyDidResult targetDidResult)
    {
        Debug.Log("PackMessage");
        JArray targetVerkeyArray = new();
        targetVerkeyArray.Add(targetDidResult.VerKey);
        string targetVerkey = targetVerkeyArray.ToString();
        Debug.Log("targetVerkey: " + targetVerkey);

        byte[] packedMessage = Crypto.PackMessageAsync(wallet, targetVerkey, didAndVerkey.VerKey,
            System.Text.Encoding.UTF8.GetBytes(message)).Result;
        Debug.Log("packedMessage: " + packedMessage.ToString());

        return Encoding.UTF8.GetString(packedMessage);
    }

    public string UnpackMessage(string message)
    {
        Debug.Log("UnpackMessage");
        byte[] unpackMessage = Crypto.UnpackMessageAsync(wallet,
            System.Text.Encoding.UTF8.GetBytes(message)).Result;
        Debug.Log("unpackMessage: " + unpackMessage.ToString());

        return Encoding.UTF8.GetString(unpackMessage);
    }
}