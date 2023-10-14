using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.IO;

using Hyperledger.Indy.WalletApi;
using Hyperledger.Indy.DidApi;
using Hyperledger.Indy.PoolApi;
using System.Net.Http;

public class IndySDK : MonoBehaviour
{
    string wallet_config;
    string wallet_credentials = "{\"key\":\"wallet_key\"}";

    public Text text;

    int wallet = 0;

    string genesis_file_path = null;
    string test_url = "http://220.68.5.139:9000/genesis";

    // Start is called before the first frame update
    void Start()
    {
        /*
        wallet_config = "{\"id\":\"wallet\", \"storage_type\": {\"path\": \"" + Application.dataPath + 
        "/.indy_client/wallet\"}}";
        */
        wallet_config = "{\"id\":\"wallet_unity\"}";
        Debug.Log(wallet_config);

        genesis_file_path = Application.dataPath + "/genesis.txn";
        HttpClient httpClient = HttpClient.GetInstance();
        string genesis_file_ = httpClient.CreateGenesisFile(genesis_file_path);
        Debug.Log("genesis_file_: " + genesis_file_);
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.V))
        {
            Debug.Log("Wallet API Test Start");
            IndyWalletApiTestFun();
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            Debug.Log("Pool API Test Start");
            IndyPoolApiTestFun();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Quit");
            Application.Quit();
        }
    }

    public void IndyWalletApiTestFun()
    {
        string wallet_name = "wallet";
        string wallet_config = "{\"id\":\"" + wallet_name + "\", \"storage_type\": {\"path\": \""
        + Application.dataPath + "/indy/wallet\"}}";
        string wallet_credentials = "{\"key\":\"wallet_key\"}";

        Wallet wallet_handle = null;
        CreateAndStoreMyDidResult did = null;
        string did_list = null;

        try
        {
            Debug.Log("Indy Create Wallet");
            Wallet.CreateWalletAsync(wallet_config, wallet_credentials).Wait();

            Debug.Log("Indy Open Wallet");
            wallet_handle = Wallet.OpenWalletAsync(wallet_config, wallet_credentials).Result;
            Debug.Log("Wallet Handle: " + wallet_handle.ToString());

            Debug.Log("Indy Create DID");
            string did_json = "{\"seed\":\"test0000000000000000000000000000\"}";
            did = Did.CreateAndStoreMyDidAsync(wallet_handle, did_json).Result;
            Debug.Log("DID: " + did);

            Debug.Log("Indy List DID");
            did_list = Did.ListMyDidsWithMetaAsync(wallet_handle).Result;
            Debug.Log("DID List: " + did_list);

            text.text = did_list;
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
        finally
        {
            Debug.Log("Indy Close Wallet");
            wallet_handle.CloseAsync().Wait();

            Debug.Log("Indy Delete Wallet");
            Wallet.DeleteWalletAsync(wallet_config, wallet_credentials).Wait();
        }
    }

    public void IndyPoolApiTestFun()
    {
        if (false == File.Exists(genesis_file_path))
        {
            Debug.Log("Genesis File is Null");
            return;
        }

        string pool_name = "pool";
        string pool_config = "{\"genesis_txn\":\"" + genesis_file_path + "\"}";
        Debug.Log("Pool Config: " + pool_config);

        Pool pool_handle = null;

        try
        {
            Debug.Log("Indy Create Pool Ledger Config");
            Pool.CreatePoolLedgerConfigAsync(pool_name, pool_config).Wait();

            Debug.Log("Indy Open Pool Ledger");
            pool_handle = Pool.OpenPoolLedgerAsync(pool_name, pool_config).Result;
            Debug.Log("Pool Handle: " + pool_handle.ToString());

            text.text = pool_handle.ToString();
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
        finally
        {
            Debug.Log("Indy Close Pool Ledger");
            pool_handle.CloseAsync().Wait();
            Debug.Log("Indy Delete Pool Ledger Config");
            Pool.DeletePoolLedgerConfigAsync(pool_name).Wait();
        }
    }
}