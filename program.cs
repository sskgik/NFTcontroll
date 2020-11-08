using Miyabi.ClientSdk;
using Miyabi.Common.Models;
using Miyabi.NFT.Models;
using Miyabi.NFT.Client;
using Miyabi.Cryptography;
using System;
using System.Threading.Tasks;
using Utility;




namespace  AssetSample
{
    class Program
    {
        const string TableName = "NFT_TEST";

        static async Task Main(string[] args)
        {

            var config = new SdkConfig(Utils.ApiUrl);
            var client = new Client(config);
            var _generalClient = new GeneralApi(client);
            // Ver2 implements module system. To enable modules, register is required.
            NFTTypesRegisterer.RegisterTypes();

            //string txID_1 = await CreateNFTTable(client);
            //string txID_2 = await NFTgenerate(client);
            //string txID_3 = await NFTSend(client);
            await ShowNFT(client);
            //Console.WriteLine($"txid={txID_1}");
            //Console.WriteLine($"txid={txID_2}");
            //Console.WriteLine($"txid={txID_3}");

            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }
        /// <summary>
        ///  Make NFT Table Method
        /// </summary>
        /// <param name="client"></param>
        /// <returns>tx.Id</returns>
        private static async Task<string> CreateNFTTable(IClient client)
        {

            //Get PublicKey from Utils's GetOwnerKeyPair()
            var tableownerAddress = new PublicKeyAddress(Utils.GetOwnerKeyPair().PublicKey);
            var assetTable = new CreateTable(new NFTTableDescriptor(
               TableName, false, false, new[] { tableownerAddress }));

            //var memo = new MemoEntry(new[] { "NFT_TEST" });
            var tx = TransactionCreator.SimpleSignedTransaction(
                new ITransactionEntry[] { assetTable },
                new[] { Utils.GetTableAdminKeyPair().PrivateKey });

            await SendTransaction(tx);

            return tx.Id.ToString();
        }
        /// <summary>
        /// Generation NFT 
        /// </summary>
        /// <param name="client"></param>
        /// <returns>tx.Id</returns>
        public static async Task<string>  NFTgenerate(IClient client)
        {
            //Asset infomation 
            Console.WriteLine("Please Types NFT TokenID");
            string Tokenid = Console.ReadLine();
            var generateNFT =new NFTAdd(TableName,Tokenid ,
            new PublicKeyAddress(Utils.GetUser0KeyPair().PublicKey));

            var tx = TransactionCreator.SimpleSignedTransaction(
                new ITransactionEntry[] { generateNFT },
                new[] { Utils.GetOwnerKeyPair().PrivateKey });


            await SendTransaction(tx);

            return tx.Id.ToString(); ;
        }
        /// <summary>
        /// Send Asset Method
        /// </summary>
        /// <param name="client"></param>
        /// <returns>tx.Id</returns>
        public static async Task<string>  NFTSend(IClient client)
        {
            var _generalClient = new GeneralApi(client);
            Console.WriteLine("Please Types NFT TokenID");
            string Tokenid = Console.ReadLine();
            var to = new PublicKeyAddress(Utils.GetUser1KeyPair());
            //enter the send amount  

            var moveCoin = new NFTMove(TableName, Tokenid, to);
            var tx = TransactionCreator.SimpleSignedTransaction(
                new ITransactionEntry[] { moveCoin },
                new [] {Utils.GetUser0KeyPair().PrivateKey});//Sender Privatekey
            await SendTransaction(tx);
            var result = await Utils.WaitTx(_generalClient,  tx.Id);
            return  result;
        }

         /// <summary>
         /// show  NFT of  designated publickeyAddress
         /// </summary>
         /// <param name="client"></param>
        private static async Task ShowNFT(IClient client)
        {
            // NFTClient has access to asset endpoints
            var nftClient = new NFTClient(client);
            var address = new PublicKeyAddress(Utils.GetUser1KeyPair());
            var result =await nftClient.GetBalanceAsync(TableName,address);
            Console.WriteLine(result.Value);
           // var result2 = await nftClient.GetNFTTableAsync(TableName);
            //Console.WriteLine(result2.Value);
            //NFT reserch tokenID
            Console.WriteLine("Please Types NFT TokenID");
            string Tokenid = Console.ReadLine();
            var result3 = await nftClient.GetOwnerOfAsync(TableName,Tokenid);
            Console.WriteLine(result3.Value);
        }
        
         /// <summary>
         /// Send Transaction to miyabi blockchain
         /// </summary>
         /// <param name="tx"></param>
        public static async Task SendTransaction(Transaction tx)
        {
            var config = new SdkConfig(Utils.ApiUrl);
            var client = new Client(config);
            var _generalClient = new GeneralApi(client);


            try
            {
                var send = await _generalClient.SendTransactionAsync(tx);
                var result_code = send.Value;
                Console.WriteLine("{0}", result_code);
                if (result_code != TransactionResultCode.Success
                   && result_code != TransactionResultCode.Pending)
                {
                    Console.WriteLine("取引が拒否されました!:\t{0}", result_code);

                }
            }
            catch (Exception e)
            {
                Console.Write("例外を検知しました！{e}");
            }

        }
    }
}
