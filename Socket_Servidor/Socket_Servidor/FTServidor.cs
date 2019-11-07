using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace Socket_Servidor
{
    public class FTServidor
    {
        static IPEndPoint ipEnd_servidor;
        static Socket sock_Servidor;
        public static string caminhoRecepcaoArquivos = @"D:\Projetos\compartilhamento-arquivos-socket-dot-net\arquivos\";
        public static string mensagemServidor = "Serviço encerrado !!";

        public static void IniciarServidor()
        {
            string strEnderecoIP = "127.0.0.1";
            ipEnd_servidor = new IPEndPoint(IPAddress.Parse(strEnderecoIP), 5052);
            sock_Servidor = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            sock_Servidor.Bind(ipEnd_servidor);

            try
            {
                sock_Servidor.Listen(1);
                mensagemServidor = "Servidor em atendimento e aguardando para receber arquivo...";
                Socket clienteSock = sock_Servidor.Accept();
                clienteSock.ReceiveBufferSize = 16384;

                byte[] dadosCliente = new byte[1024 * 50000];

                int tamanhoBytesRecebidos = clienteSock.Receive(dadosCliente, dadosCliente.Length, 0);
                int tamnhoNomeArquivo = BitConverter.ToInt32(dadosCliente, 0);
                string nomeArquivo = Encoding.UTF8.GetString(dadosCliente, 4, tamnhoNomeArquivo);

                BinaryWriter bWrite = new BinaryWriter(File.Open(caminhoRecepcaoArquivos + nomeArquivo, FileMode.Append));
                bWrite.Write(dadosCliente, 4 + tamnhoNomeArquivo, tamanhoBytesRecebidos - 4 - tamnhoNomeArquivo);


                while (tamanhoBytesRecebidos > 0)
                {
                    tamanhoBytesRecebidos = clienteSock.Receive(dadosCliente, dadosCliente.Length, 0);
                    if (tamanhoBytesRecebidos == 0)
                    {
                        bWrite.Close();
                    }
                    else
                    {
                        bWrite.Write(dadosCliente, 0, tamanhoBytesRecebidos);
                    }
                }
                bWrite.Close();

                clienteSock.Close();
                mensagemServidor = "Arquivo recebido e arquivado [" + nomeArquivo + "] (" + (tamanhoBytesRecebidos - 4 - tamnhoNomeArquivo) + " bytes recebido); Servidor Parado";
            }
            catch (SocketException ex)
            {
                mensagemServidor = ex.Message +  " - Erro ao receber arquivo.";
            }
        }
    }
}
