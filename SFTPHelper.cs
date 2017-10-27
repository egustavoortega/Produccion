using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Providers.Entities;
using DotNetOpenAuth.Messaging;
using Tamir.SharpSsh.jsch;

namespace ControlesDinamicos
{
    public class SFTPHelper
    {
        private Tamir.SharpSsh.jsch.Session m_session;

        private Tamir.SharpSsh.jsch.Channel m_channel;

        private ChannelSftp m_sftp;



        //Host: SFTP: nombre de usuario contraseña de usuario: Dirección de pwd        

        public SFTPHelper(string host, string user, string pwd)
        {

            string[] arr = host.Split(':');

            string ip = arr[0];

            int port = 22;

            if (arr.Length > 1) port = Int32.Parse(arr[1]);



            JSch jsch = new JSch();

            m_session = jsch.getSession(user, ip, port);

            MyUserInfo ui = new MyUserInfo();

            ui.setPassword(pwd);

            m_session.setUserInfo(ui);



        }



        //Estado de conexión SFTP        

        public bool Connected { get { return m_session.isConnected(); } }



        //Conexión SFTP        

        public bool Connect()
        {

            try
            {

                if (!Connected)
                {

                    m_session.connect();

                    m_channel = m_session.openChannel("sftp");

                    m_channel.connect();

                    m_sftp = (ChannelSftp)m_channel;

                }

                return true;

            }

            catch
            {

                return false;

            }

        }



        //Desconectar SFTP        

        public void Disconnect()
        {

            if (Connected)
            {

                m_channel.disconnect();

                m_session.disconnect();

            }

        }



        //Archivo de SFTP        

        public bool Put(string localPath, string remotePath)
        {

            try
            {

                Tamir.SharpSsh.java.String src = new Tamir.SharpSsh.java.String(localPath);

                Tamir.SharpSsh.java.String dst = new Tamir.SharpSsh.java.String(remotePath);

                m_sftp.put(src, dst);

                return true;

            }

            catch
            {

                return false;

            }

        }



        //La obtención de documentos de SFTP        

        public bool Get(string remotePath, string localPath)
        {

            try
            {

                Tamir.SharpSsh.java.String src = new Tamir.SharpSsh.java.String(remotePath);

                Tamir.SharpSsh.java.String dst = new Tamir.SharpSsh.java.String(localPath);

                m_sftp.get(src, dst);

                return true;

            }

            catch
            {

                return false;

            }

        }

        //Borrar el archivo de SFTP

        public bool Delete(string remoteFile)
        {

            try
            {

                m_sftp.rm(remoteFile);

                return true;

            }

            catch
            {

                return false;

            }

        }



        //Para obtener la lista de archivos de SFTP        

        public ArrayList GetFileList(string remotePath, string fileType)
        {

            try
            {

                Tamir.SharpSsh.java.util.Vector vvv = m_sftp.ls(remotePath);

                ArrayList objList = new ArrayList();

                foreach (Tamir.SharpSsh.jsch.ChannelSftp.LsEntry qqq in vvv)
                {

                    string sss = qqq.getFilename();

                    if (sss.Length > (fileType.Length + 1) && fileType == sss.Substring(sss.Length - fileType.Length))

                    { objList.Add(sss); }

                    else { continue; }

                }



                return objList;

            }

            catch
            {

                return null;

            }

        }





        //La verificación de la información de inicio de sesión        

        public class MyUserInfo : UserInfo
        {

            String passwd;

            public String getPassword() { return passwd; }

            public void setPassword(String passwd) { this.passwd = passwd; }



            public String getPassphrase() { return null; }

            public bool promptPassphrase(String message) { return true; }



            public bool promptPassword(String message) { return true; }

            public bool promptYesNo(String message) { return true; }

            public void showMessage(String message) { }

        }
    }
}