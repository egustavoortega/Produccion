using EHF;
//using Repote;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Traking_Forms;
using System.Windows.Media.Imaging;
using System.Drawing.Drawing2D;
using System.Text.RegularExpressions;
using System.Net;
using Recorte;
using System.Globalization;

namespace BasicAPI
{
    #region Registro DLL
    [Guid("25E74A0B-C5DC-4551-8DE1-A311B16BD1A2"), ClassInterface(ClassInterfaceType.AutoDual), ProgId("TrakingAPIFormsClass")]
    [assembly: AssemblyDelaySign(false)]
    #endregion


    public class APIFormsClass
    {


        #region Declaracion Forms

        [DllImport("user32.dll")]
        public static extern int FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        public static extern int SendMessage(int hWnd, uint Msg, int wParam, int lParam);

        [DllImport("KERNEL32.DLL", EntryPoint = "GetPrivateProfileStringW", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern int GetPrivateProfileString(
            string lpAppName,
            string lpKeyName,
            string lpDefault,
            string lpReturnString,
            int nSize,
            string lpFilename
        );

        EHF.Application EHFApp;
        String _Ehlocal;
        SqlConnection conexionTrans;
        bool Retype_codtrans = false;
        bool Masa_codtrans = true;
        bool NoDivipol = false;

        Dictionary<string, object> parametros;
        string ipEhGlobal;
        int ids_documents;
        string sUrlRequest = "";
        bool Captura22 = false;
        bool EstadoNom1 = false;
        bool EstadoNom2 = false;
        bool EstadoApe1 = false;
        bool EstadoApe2 = false;
        bool EstadoFecha = false;
        bool EstadoBloqueo = false;
        SqlConnection conexion;








        #endregion

        #region Eventos Forms

        public int Connect(Object objEHFApp, String sIniFile, String sIniSection)
        {
            _Ehlocal = sIniFile;
            EHFApp = App.CreateApp(objEHFApp);

            MessageBox.Show("Traking FORMS V - E3 -  Version: " + Assembly.GetExecutingAssembly().GetName().Version.ToString(), "Connect", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

            conexion = GetConexion();
            conexion.Open();
            CargasIniciales();

            if (EHFApp is ManagerApp)
            {
                EHFApp.Subscribe(this, "AppUserDefined1", "OnMngrAppUserDefined1");
                EHFApp.Subscribe(this, "AppUserDefined2", "OnMngrAppUserDefined2");
                EHFApp.Subscribe(this, "AppUserDefined3", "OnMngrAppUserDefined3");
                EHFApp.Subscribe(this, "AppStarted", "OnAppStarted");
                EHFApp.Subscribe(this, "AppCanClose", "OnAppCanClose");
                EHFApp.Subscribe(this, "StopJob", "OnStopJob");
                GC.Collect();
                return (int)ehReturnValue.EV_OK;

            }
            else if (EHFApp is InterpretApp)
            {
                EHFApp.Subscribe(this, "AppCanClose", "OnAppCanClose");
                EHFApp.Subscribe(this, "FormInterpreted", "OnFormInterpreted");
                EHFApp.Subscribe(this, "FormValidated", "OnFormValidated");
                EHFApp.Subscribe(this, "StopJob", "OnStopJob");
                GC.Collect();
                return (int)ehReturnValue.EV_OK;

            }
            else if (EHFApp is VerifyApp)
            {
                EHFApp.Subscribe(this, "StopJob", "OnStopJob");
                EHFApp.Subscribe(this, "AppCanClose", "OnAppCanClose");

                EHFApp.Subscribe(this, "AppUserDefined1", "OnVeriAppUserDefined1");
                EHFApp.Subscribe(this, "FormValidate", "OnFormValidate");
                EHFApp.Subscribe(this, "FormComplete", "OnFormComplete");


                EHFApp.Subscribe(this, "FieldComplete", "OnFieldComplete");
                EHFApp.Subscribe(this, "FieldRetyped", "OnFieldRetyped");


                EHFApp.Subscribe(this, "FieldVerify", "OnFieldVerify");

                EHFApp.Subscribe(this, "ForceFieldComplete", "OnForceFieldComplete");
                EHFApp.Subscribe(this, "FieldValidateNonExisting", "OnFieldValidateNonExisting");
                EHFApp.Subscribe(this, "FieldValidateExisting", "OnFieldValidateExisting");
                EHFApp.Subscribe(this, "FieldValidateError", "OnFieldValidateError");

                EHFApp.Subscribe(this, "CompFieldStartExtVerify", "onCompFieldStartExtVerify");

                EHFApp.Subscribe(this, "AppStarted", "OnAppStarted");
                // this.DeshabilitarMenues();
                GC.Collect();
                return (int)ehReturnValue.EV_OK;


            }
            else if (EHFApp is TransferApp)
            {
                EHFApp.Subscribe(this, "AppCanClose", "OnAppCanClose");
                EHFApp.Subscribe(this, "StopJob", "OnStopJob");
                EHFApp.Subscribe(this, "FormTransferEnd", "OnFormTransferEnd");
                GC.Collect();
                return (int)ehReturnValue.EV_OK;
            }

            conexion.Close();
            conexion.Dispose();
            GC.Collect();
            return (int)ehReturnValue.EV_OK;
        }


        public int OnFieldValidateExisting()
        {
            try
            {
                GC.Collect();
                return (int)ehReturnValue.EV_OK;

            }
            catch (Exception err)
            {
                Log(err.ToString());
                MessageBox.Show(err.Message + " ## " + err.InnerException != null ? err.InnerException.Message : string.Empty + " ## " + err.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                GC.Collect();
                return (int)ehReturnValue.EV_OK_ERROR;
            }
        }

        public int OnFieldValidateError()
        {

            string nom = EHFApp.Field.GetName();
            string xxx = EHFApp.Field.GetValueStr();
            if ((nom == "TipoIden") || (nom == "Genero") || (nom == "Ubicacion") || (nom == "TipoSol") || (nom == "Regimen") || (nom == "Traslado") || (nom == "DeclaDatos"))
            {
                if (xxx == "")
                    EHFApp.Field.SetStatus(ehFieldStatus.ehComplete);
            }
            else
            {
                //EHFApp.Field.SetValueStr("");
                bool Resultado = false;
                Resultado = ValidacaionDeCampos(nom, xxx);
                if (Resultado)
                {
                    GC.Collect();
                    return (int)ehReturnValue.EV_OK;
                }
                else
                {
                    GC.Collect();
                    return (int)ehReturnValue.EV_OK_ABORT;
                }

            }

            GC.Collect();
            return (int)ehReturnValue.EV_OK;
        }


        public int onCompFieldStartExtVerify()
        {
            try
            {

                //string nom = EHFApp.Field.GetName();

                //if (EHFApp is VerifyApp)
                //{
                //    if (EHFApp.Job.GetName().ToUpper().Contains("PRIMERA"))
                //    {
                //        if (EHFApp.Field.GetStatus() != ehFieldStatus.ehComplete)
                //        {


                //          //  EHFApp.Field.SetStatus(ehFieldStatus.ehComplete);
                //        }


                //    }
                //}
                GC.Collect();
                return (int)ehReturnValue.EV_OK;
            }
            catch (Exception err)
            {
                Log(err.ToString());
                // MessageBox.Show(err.Message + " ## " + err.InnerException != null ? err.InnerException.Message : string.Empty + " ## " + err.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                GC.Collect();
                return (int)ehReturnValue.EV_OK_ERROR;
            }
        }

        private void CargasIniciales()
        {
            try
            {
                Dictionary<string, object> parametros = new Dictionary<string, object>();
                parametros.Add("@Name", "URL");
                parametros.Add("@opcion", "1");
                DataTable dt = ExecuteSPCon(conexion, "[dbo].[Get_Traking_Set]", parametros);

                sUrlRequest = dt.Rows[0][0].ToString();
                GC.Collect();
            }
            catch (Exception x)
            {
                Log(x.ToString());
                GC.Collect();
            }
        }

        public int OnFieldComplete()
        {
            try
            {

                bool Resultado = false;
                string campo = EHFApp.Field.GetName().ToString();
                string valor = EHFApp.Field.GetValueStr().ToString();
                string estado1 = EHFApp.Field.GetStatus() == ehFieldStatus.ehComplete ? "1" : "0";

                if (EHFApp.Job.GetName().ToUpper().Contains("PRIMERA"))
                {
                    // OnFieldRetyped(valor);

                    #region No REQUERIDOS
                    if (campo == "cedula")
                    {
                        Resultado = ValidacaionDeCampos(campo, valor);
                        if (Resultado)
                        {

                            if (valor == "")
                            {
                                EHFApp.Field.SetValueStr(valor);
                                MessageBox.Show("El Campo no permite valores en blanco", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                GC.Collect();
                                return (int)ehReturnValue.EV_OK_ABORT;
                            }
                            else
                            {

                                #region Cedula con datos

                                if (valor.Length < 5)
                                {
                                    Resultado = false;
                                }
                                else if (valor.Length == 5)
                                {
                                    if (valor == "00000")
                                    {
                                        Resultado = true;
                                        EHFApp.Form.GetFormField(campo, 0).SetStatus(ehFieldStatus.ehComplete);
                                    }
                                    else
                                    {
                                        Resultado = false;
                                    }
                                }
                                else
                                {

                                    if (valor == "")
                                    {
                                        EHFApp.Field.SetValueStr(valor);
                                        GC.Collect(); return (int)ehReturnValue.EV_OK_ABORT;
                                    }

                                }

                                #endregion
                            }

                        }
                        else
                        {
                            ClassData.NumeroIntentosCaptura2++;
                            MessageBox.Show("Valor Digitado no valido", "Notificacion", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            EHFApp.Field.SetValueStr("");
                            GC.Collect();
                            return (int)ehReturnValue.EV_OK_ABORT;
                        }
                    }
                    else if (campo.ToUpper() == "ANULADO")
                    {
                        Resultado = ValidacaionDeCampos(campo, valor);
                        if (Resultado)
                        {
                            if (valor.ToUpper() == "SI")
                            {
                                for (short i = 1; i <= EHFApp.Form.GetMaxNoOfFormFields(); i++)
                                {
                                    campo = EHFApp.Form.GetFormFieldNo(i).GetName().Trim();
                                    valor = EHFApp.Form.GetFormFieldNo(i).GetValueStr().ToUpper().Trim();
                                    string estado = EHFApp.Form.GetFormFieldNo(i).GetStatus() == ehFieldStatus.ehComplete ? "1" : "0";


                                    if (campo == "Anulado")
                                    {
                                        EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehValidationError);
                                    }
                                    else
                                    {
                                        EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                                    }


                                }
                            }
                        }
                        else
                        {
                            ClassData.NumeroIntentosCaptura2++;
                            MessageBox.Show("Valor Digitado no valido", "Notificacion", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            EHFApp.Field.SetValueStr("");
                            GC.Collect();
                            return (int)ehReturnValue.EV_OK_ABORT;
                        }


                    }
                    else
                    {

                        Resultado = ValidacaionDeCampos(campo, valor);
                        if (!Resultado)
                        {
                            #region Fecha inscripcion
                            if (campo.ToUpper().StartsWith("FI"))
                            {
                                if (valor.Length == 2)
                                {
                                    if (valor == "00")
                                    {
                                        EHFApp.Form.GetFormField("finsdia", 0).SetValueStr("00");
                                        EHFApp.Form.GetFormField("finsmes", 0).SetValueStr("00");
                                        EHFApp.Form.GetFormField("finsano", 0).SetValueStr("0");
                                        EHFApp.Form.GetFormField("finsdia", 0).SetStatus(ehFieldStatus.ehComplete);
                                        EHFApp.Form.GetFormField("finsmes", 0).SetStatus(ehFieldStatus.ehComplete);
                                        EHFApp.Form.GetFormField("finsano", 0).SetStatus(ehFieldStatus.ehComplete);
                                        Resultado = true;
                                    }
                                }
                            }
                            #endregion
                            #region Fecha Expedicion
                            if (campo.ToUpper().StartsWith("FE"))
                            {
                                if (valor.Length == 2)
                                {
                                    if (valor == "00")
                                    {

                                        EHFApp.Form.GetFormField("fexpedia", 0).SetValueStr("00");
                                        EHFApp.Form.GetFormField("fexpemes", 0).SetValueStr("00");
                                        EHFApp.Form.GetFormField("fexpeano", 0).SetValueStr("0000");
                                        EHFApp.Form.GetFormField("fexpedia", 0).SetStatus(ehFieldStatus.ehComplete);
                                        EHFApp.Form.GetFormField("fexpemes", 0).SetStatus(ehFieldStatus.ehComplete);
                                        EHFApp.Form.GetFormField("fexpeano", 0).SetStatus(ehFieldStatus.ehComplete);
                                        Resultado = true;
                                    }
                                }
                            }
                            #endregion

                            if (Resultado)
                            {
                                ClassData.NumeroIntentosCaptura2 = 0;
                                ClassData.VlrAprobado = "";
                                EHFApp.Field.SetValueStr(valor);
                                EHFApp.Field.SetStatus(ehFieldStatus.ehComplete);
                                EHFApp.SetReturnValue(0, "Y");
                                GC.Collect();
                                return (int)ehReturnValue.EV_OK;
                            }
                            else
                            {
                                ClassData.NumeroIntentosCaptura2++;
                                //MessageBox.Show("Valor Digitado no valido", "Notificacion", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                if (campo != "TelFijo" && campo != "TelMovil")
                                {
                                    EHFApp.Field.SetValueStr(valor);
                                }

                                EHFApp.Field.SetStatus(ehFieldStatus.ehValidationError);
                                GC.Collect();
                                return (int)ehReturnValue.EV_OK_ABORT;
                            }
                        }
                        else
                        {
                            ClassData.NumeroIntentosCaptura2 = 0;
                            ClassData.VlrAprobado = "";
                            EHFApp.Field.SetStatus(ehFieldStatus.ehComplete);
                            EHFApp.SetReturnValue(0, "Y");
                            GC.Collect();
                            return (int)ehReturnValue.EV_OK;
                        }
                    }
                    #endregion

                }
                else if (EHFApp.Job.GetName().ToUpper().Contains("SEGUNDA"))
                {
                    #region No REQUERIDOS
                    if (campo == "cedula")
                    {
                        Resultado = ValidacaionDeCampos(campo, valor);
                        if (Resultado)
                        {

                            if (valor == "")
                            {
                                EHFApp.Field.SetValueStr(valor);
                                MessageBox.Show("El Campo no permite valores en blanco", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                GC.Collect();
                                return (int)ehReturnValue.EV_OK_ABORT;
                            }
                            else
                            {

                                #region Cedula con datos

                                if (valor.Length < 5)
                                {
                                    Resultado = false;
                                }
                                else if (valor.Length == 5)
                                {
                                    if (valor == "00000")
                                    {
                                        Resultado = true;
                                        EHFApp.Form.GetFormField(campo, 0).SetStatus(ehFieldStatus.ehComplete);
                                    }
                                    else
                                    {
                                        Resultado = false;
                                    }
                                }
                                else
                                {

                                    if (valor == "")
                                    {
                                        EHFApp.Field.SetValueStr(valor);
                                        GC.Collect(); return (int)ehReturnValue.EV_OK_ABORT;
                                    }

                                }

                                #endregion
                            }

                        }
                        else
                        {
                            ClassData.NumeroIntentosCaptura2++;
                            MessageBox.Show("Valor Digitado no valido", "Notificacion", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            EHFApp.Field.SetValueStr("");
                            GC.Collect();
                            return (int)ehReturnValue.EV_OK_ABORT;
                        }
                    }
                    else if (campo.ToUpper() == "ANULADO")
                    {
                        Resultado = ValidacaionDeCampos(campo, valor);
                        if (Resultado)
                        {
                            if (valor.ToUpper() == "SI")
                            {
                                ClassData.Braile = false;
                                ClassData.Invidente = false;
                                ClassData.Nvlestudio = false;
                                ClassData.Genero = false;
                                ClassData.Capacidad = false;
                                DataTable ObjTrazabilidad = GetDataTableBasico();
                                string Nombre = EHFApp.Form.GetEHKey();
                                string usuario = EHFApp.GetUserName().Trim();
                                string Cedula = "";

                                for (short i = 1; i <= EHFApp.Form.GetMaxNoOfFormFields(); i++)
                                {

                                    campo = EHFApp.Form.GetFormFieldNo(i).GetName().Trim();
                                    valor = EHFApp.Form.GetFormFieldNo(i).GetValueStr().ToUpper().Trim();
                                    string estado = EHFApp.Form.GetFormFieldNo(i).GetStatus() == ehFieldStatus.ehComplete ? "1" : "0";

                                    valor = "";
                                    EHFApp.Form.GetFormFieldNo(i).SetValueStr(string.Empty);
                                    EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                                    if (campo != "Anulado")
                                    {

                                        if (campo == "Braile" || campo == "Nvlestudio" || campo == "Invidente" || campo == "Genero" || campo == "Capacidad")
                                        {

                                            EHFApp.Form.GetFormFieldNo(i).SetValueStr(string.Empty);
                                            EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                                            valor = ValidarCampos(campo, valor, ObjTrazabilidad);
                                            if (valor != "%")
                                            {
                                                ObjTrazabilidad.Rows.Add(campo, "", "0", usuario);
                                            }
                                        }
                                        else
                                        {
                                            // EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                                            // EHFApp.Form.SetStatus(ehFormStatus.ehInterpretError);
                                            ObjTrazabilidad.Rows.Add(campo, valor, estado, usuario);
                                        }
                                    }
                                    else
                                    {
                                        EHFApp.Form.GetFormField("Anulado", 0).SetValueStr(string.Empty);
                                        EHFApp.Form.GetFormField("Anulado", 0).SetStatus(ehFieldStatus.ehComplete);
                                        ObjTrazabilidad.Rows.Add(campo, "SI", "1", usuario);
                                    }

                                    //  EHFApp.Form.GetFormFieldNo(i).SetValueStr(string.Empty);

                                }

                                ObjTrazabilidad.AcceptChanges();
                                ExcuteTrazabilidad(Nombre, ObjTrazabilidad, 2);

                                //EHFApp.Form.SetStatus(ehFormStatus.ehComplete);
                                //EHFApp.Form.SetQueues(2);
                                //(EHFApp as VerifyApp).SkipForm(1);
                                GC.Collect();
                                return (int)ehReturnValue.EV_OK;
                            }
                        }
                        else
                        {
                            ClassData.NumeroIntentosCaptura2++;
                            MessageBox.Show("Valor Digitado no valido", "Notificacion", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            EHFApp.Field.SetValueStr("");
                            GC.Collect();
                            return (int)ehReturnValue.EV_OK_ABORT;
                        }


                    }
                    else
                    {
                        if (campo == "Nom1" || campo == "Nom2" || campo == "Ape1" || campo == "Ape2" || campo == "fexpedia"
                            || campo == "fexpemes" || campo == "fexpeano" || campo == "finsdia" || campo == "finsmes" || campo == "finsano")
                        {
                            Resultado = ValidacaionDeCampos(campo, valor);
                            if (Resultado)
                            {

                                #region Campos Reuqeridos

                                string campoNuevo = campo;
                                string opcion = "0";
                                if (campo == "fexpedia" || campo == "fexpemes" || campo == "fexpeano")
                                {
                                    if (campo == "fexpedia")
                                    {
                                        opcion = "1";
                                    }
                                    else if (campo == "fexpemes")
                                    {
                                        opcion = "2";
                                    }
                                    else
                                    {
                                        opcion = "3";
                                    }

                                    campoNuevo = "FechaExpedicion";
                                }
                                if (campo == "finsdia" || campo == "finsmes" || campo == "finsano")
                                {
                                    if (campo == "finsdia")
                                    {
                                        opcion = "1";
                                    }
                                    else if (campo == "finsmes")
                                    {
                                        opcion = "2";
                                    }
                                    else
                                    {
                                        opcion = "3";
                                    }
                                    campoNuevo = "FechaInscripcion";
                                }
                                if (campo == "Braile")
                                {
                                    campoNuevo = "Braille";
                                }

                                #region Ventana
                                string codigoBarras = GetParametroIni("ventana");
                                if (codigoBarras == "1")
                                {
                                    string nombreDoc = EHFApp.Form.GetEHKey();
                                    Dictionary<string, object> parametros = new Dictionary<string, object>();
                                    parametros.Add("Nombre", nombreDoc);
                                    parametros.Add("campoForms", campo);
                                    parametros.Add("campo", campoNuevo);
                                    parametros.Add("opcion", opcion);
                                    DataTable dt = ExecuteSPCon(conexion, "[dbo].[Traking_Cap_Anteriores2]", parametros);
                                    if (!validarSegundaCaptura(dt, valor))
                                    {
                                        int Nintento = int.Parse(GetParametroIni("NumeroIntentos"));
                                        if (ClassData.NumeroIntentosCaptura2 >= Nintento)
                                        {
                                            MensajeCorregir obj = new MensajeCorregir();
                                            obj.ShowDialog();
                                            GC.Collect();
                                            if (ClassData.VlrAprobado != "")
                                            {
                                                ClassData.NumeroIntentosCaptura2 = 0;



                                                EHFApp.Form.GetFormField("Nvlestudio", 2).SetValueStr("");
                                                EHFApp.Form.GetFormField("Nvlestudio", 2).SetStatus(ehFieldStatus.ehComplete);
                                                EHFApp.Form.GetFormField("Nvlestudio", 3).SetValueStr("");
                                                EHFApp.Form.GetFormField("Nvlestudio", 3).SetStatus(ehFieldStatus.ehComplete);
                                                EHFApp.Form.GetFormField("Nvlestudio", 4).SetValueStr("");
                                                EHFApp.Form.GetFormField("Nvlestudio", 4).SetStatus(ehFieldStatus.ehComplete);
                                                EHFApp.Form.GetFormField("Genero", 2).SetValueStr("");
                                                EHFApp.Form.GetFormField("Genero", 2).SetStatus(ehFieldStatus.ehComplete);
                                                EHFApp.Form.GetFormField("Capacidad", 2).SetValueStr("");
                                                EHFApp.Form.GetFormField("Genero", 2).SetStatus(ehFieldStatus.ehComplete);



                                                EHFApp.Field.SetValueStr(ClassData.VlrAprobado);
                                                ClassData.VlrAprobado = "";
                                                EHFApp.Field.SetStatus(ehFieldStatus.ehComplete);
                                                //OnFieldComplete();
                                                EHFApp.SetReturnValue(0, "Y");
                                                GC.Collect();
                                                return (int)ehReturnValue.EV_OK;
                                            }
                                            else
                                            {
                                                EHFApp.Field.SetValueStr("");
                                                EHFApp.Field.SetStatus(ehFieldStatus.ehValidationError);
                                                GC.Collect();
                                                return (int)ehReturnValue.EV_OK_ABORT;
                                            }
                                        }
                                        else
                                        {
                                            ClassData.NumeroIntentosCaptura2++;
                                            EHFApp.Field.SetStatus(ehFieldStatus.ehValidationError);
                                            EHFApp.Field.SetValueStr("");
                                            GC.Collect();
                                            return (int)ehReturnValue.EV_OK_ABORT;
                                        }

                                    }
                                    else
                                    {
                                        ClassData.NumeroIntentosCaptura2++;
                                    }
                                }
                                #endregion
                                // }
                                #endregion
                            }
                            else
                            {
                                ClassData.NumeroIntentosCaptura2++;
                                MessageBox.Show("Valor Digitado no valido", "Notificacion", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                EHFApp.Field.SetValueStr("");
                                GC.Collect();
                                return (int)ehReturnValue.EV_OK_ABORT;
                            }
                        }












                        Resultado = ValidacaionDeCampos(campo, valor);
                        if (!Resultado)
                        {
                            #region Fecha inscripcion
                            if (campo.ToUpper().StartsWith("FI"))
                            {
                                if (valor.Length == 2)
                                {
                                    if (valor == "00")
                                    {
                                        EHFApp.Form.GetFormField("finsdia", 0).SetValueStr("00");
                                        EHFApp.Form.GetFormField("finsmes", 0).SetValueStr("00");
                                        EHFApp.Form.GetFormField("finsano", 0).SetValueStr("0");
                                        EHFApp.Form.GetFormField("finsdia", 0).SetStatus(ehFieldStatus.ehComplete);
                                        EHFApp.Form.GetFormField("finsmes", 0).SetStatus(ehFieldStatus.ehComplete);
                                        EHFApp.Form.GetFormField("finsano", 0).SetStatus(ehFieldStatus.ehComplete);
                                        Resultado = true;
                                    }
                                }
                            }
                            #endregion
                            #region Fecha Expedicion
                            if (campo.ToUpper().StartsWith("FE"))
                            {
                                if (valor.Length == 2)
                                {
                                    if (valor == "00")
                                    {

                                        EHFApp.Form.GetFormField("fexpedia", 0).SetValueStr("00");
                                        EHFApp.Form.GetFormField("fexpemes", 0).SetValueStr("00");
                                        EHFApp.Form.GetFormField("fexpeano", 0).SetValueStr("0000");
                                        EHFApp.Form.GetFormField("fexpedia", 0).SetStatus(ehFieldStatus.ehComplete);
                                        EHFApp.Form.GetFormField("fexpemes", 0).SetStatus(ehFieldStatus.ehComplete);
                                        EHFApp.Form.GetFormField("fexpeano", 0).SetStatus(ehFieldStatus.ehComplete);
                                        Resultado = true;
                                    }
                                }
                            }
                            #endregion

                            if (Resultado)
                            {
                                ClassData.NumeroIntentosCaptura2 = 0;
                                ClassData.VlrAprobado = "";
                                EHFApp.Field.SetValueStr(valor);
                                EHFApp.Field.SetStatus(ehFieldStatus.ehComplete);
                                EHFApp.SetReturnValue(0, "Y");
                                GC.Collect();
                                return (int)ehReturnValue.EV_OK;
                            }
                            else
                            {
                                ClassData.NumeroIntentosCaptura2++;
                                //   MessageBox.Show("Valor Digitado no valido", "Notificacion", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                if (campo != "TelFijo" && campo != "TelMovil")
                                {
                                    EHFApp.Field.SetValueStr("");
                                }

                                EHFApp.Field.SetStatus(ehFieldStatus.ehValidationError);
                                GC.Collect();
                                return (int)ehReturnValue.EV_OK_ABORT;
                            }
                        }
                        else
                        {
                            ClassData.NumeroIntentosCaptura2 = 0;
                            ClassData.VlrAprobado = "";
                            EHFApp.Field.SetStatus(ehFieldStatus.ehComplete);
                            EHFApp.SetReturnValue(0, "Y");
                            GC.Collect();
                            return (int)ehReturnValue.EV_OK;
                        }
                    }
                    #endregion


                }
                //else if (campo.ToUpper() == "ANULADO")
                //{
                //    if (valor.ToUpper() == "SI")
                //    {
                //        ClassData.Braile = false;
                //        ClassData.Invidente = false;
                //        ClassData.Nvlestudio = false;
                //        ClassData.Genero = false;
                //        ClassData.Capacidad = false;
                //        DataTable ObjTrazabilidad = GetDataTableBasico();
                //        string Nombre = EHFApp.Form.GetEHKey();
                //        string usuario = EHFApp.GetUserName().Trim();
                //        string Cedula = "";

                //        for (short i = 1; i <= EHFApp.Form.GetMaxNoOfFormFields(); i++)
                //        {
                //            //if (EHFApp.Form.GetFormFieldNo(i).GetValueStr().Contains("#ErrorCaptura#"))
                //            campo = EHFApp.Form.GetFormFieldNo(i).GetName().Trim();
                //            valor = EHFApp.Form.GetFormFieldNo(i).GetValueStr().ToUpper().Trim();
                //            string estado = EHFApp.Form.GetFormFieldNo(i).GetStatus() == ehFieldStatus.ehComplete ? "1" : "0";

                //            valor = "";
                //            EHFApp.Form.GetFormFieldNo(i).SetValueStr(string.Empty);
                //            EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehRetype);


                //            if (campo != "Anulado")
                //            {

                //                if (campo == "Braile" || campo == "Nvlestudio" || campo == "Invidente" || campo == "Genero" || campo == "Capacidad")
                //                {
                //                    #region Braile
                //                    if (campo == "Braile")
                //                    {
                //                        if (ClassData.Braile)
                //                        {
                //                            valor = "";
                //                            EHFApp.Form.GetFormFieldNo(i).SetValueStr(string.Empty);
                //                            EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                //                        }
                //                        else
                //                        {
                //                            valor = "";
                //                            EHFApp.Form.GetFormFieldNo(i).SetValueStr(string.Empty);
                //                            EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehRetype); //ClassData.Braile = true;
                //                        }
                //                    }
                //                    #endregion
                //                    #region Nvlestudio

                //                    if (campo == "Nvlestudio")
                //                    {
                //                        if (ClassData.Nvlestudio)
                //                        {
                //                            valor = "";
                //                            EHFApp.Form.GetFormFieldNo(i).SetValueStr(string.Empty);
                //                            EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                //                        }
                //                        else
                //                        {
                //                            valor = "";
                //                            EHFApp.Form.GetFormFieldNo(i).SetValueStr(string.Empty);
                //                            EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehRetype); //ClassData.Nvlestudio = true;
                //                        }
                //                    }
                //                    #endregion
                //                    #region Invidente
                //                    if (campo == "Invidente")
                //                    {
                //                        if (ClassData.Invidente)
                //                        {
                //                            valor = "";
                //                            EHFApp.Form.GetFormFieldNo(i).SetValueStr(string.Empty);
                //                            EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                //                        }
                //                        else
                //                        {
                //                            valor = "";
                //                            EHFApp.Form.GetFormFieldNo(i).SetValueStr(string.Empty);
                //                            EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehRetype); //ClassData.Invidente = true;
                //                        }
                //                    }
                //                    #endregion
                //                    #region Genero
                //                    if (campo == "Genero")
                //                    {
                //                        if (ClassData.Genero)
                //                        {
                //                            valor = "";
                //                            EHFApp.Form.GetFormFieldNo(i).SetValueStr(string.Empty);
                //                            EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                //                        }
                //                        else
                //                        {
                //                            valor = "";
                //                            EHFApp.Form.GetFormFieldNo(i).SetValueStr(string.Empty);
                //                            EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehRetype); //ClassData.Genero = true;
                //                        }
                //                    }
                //                    #endregion
                //                    #region Capacidad
                //                    if (campo == "Capacidad")
                //                    {
                //                        if (ClassData.Capacidad)
                //                        {
                //                            valor = "";
                //                            EHFApp.Form.GetFormFieldNo(i).SetValueStr(string.Empty);
                //                            EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                //                        }
                //                        else
                //                        {
                //                            valor = "";
                //                            EHFApp.Form.GetFormFieldNo(i).SetValueStr(string.Empty);
                //                            EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehRetype); //ClassData.Capacidad = true;
                //                        }
                //                    }
                //                    #endregion

                //                    //EHFApp.Form.GetFormFieldNo(i).SetValueStr(string.Empty);
                //                    valor = ValidarCampos(campo, valor, ObjTrazabilidad);
                //                    if (valor != "%")
                //                    {
                //                        ObjTrazabilidad.Rows.Add(campo, "", "0", usuario);
                //                    }
                //                }
                //                else
                //                {
                //                    EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehRetype);
                //                   // EHFApp.Form.SetStatus(ehFormStatus.ehInterpretError);
                //                    ObjTrazabilidad.Rows.Add(campo, valor, estado, usuario);
                //                }
                //            }
                //            else
                //            {
                //                EHFApp.Form.GetFormField("Anulado", 0).SetValueStr(string.Empty);
                //                EHFApp.Form.GetFormField("Anulado", 0).SetStatus(ehFieldStatus.ehInterpretError);
                //                ObjTrazabilidad.Rows.Add(campo, "SI", "1", usuario);
                //            }

                //            //  EHFApp.Form.GetFormFieldNo(i).SetValueStr(string.Empty);

                //        }

                //        ObjTrazabilidad.AcceptChanges();
                //        ExcuteTrazabilidad(Nombre, ObjTrazabilidad, 2);
                //        EHFApp.Form.SetStatus(ehFormStatus.ehValidationError);

                //        if (EHFApp.Form.GetQueues() == 1)
                //        {
                //            EHFApp.Form.SetQueues(2);
                //        }
                //        else if (EHFApp.Form.GetQueues() == 2)
                //        {
                //            EHFApp.Form.SetQueues(4);
                //        }
                //        else
                //        {
                //            EHFApp.Form.SetStatus(ehFormStatus.ehComplete);
                //            EHFApp.Form.SetQueues(0);
                //        }
                //        OnFieldVerify();
                //        (EHFApp as VerifyApp).SkipForm(1);

                //    }
                //    GC.Collect();
                //    return (int)ehReturnValue.EV_OK;

                //}



                if (EHFApp.Field.GetValueStr() == "")
                {
                    MessageBox.Show("Campo No Valido", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    GC.Collect();
                    return (int)ehReturnValue.EV_OK_ABORT;

                }

                GC.Collect();
                return (int)ehReturnValue.EV_OK;

            }
            catch (Exception err)
            {
                Log(err.ToString());
                MessageBox.Show(err.Message + " ## " + err.InnerException != null ? err.InnerException.Message : string.Empty + " ## " + err.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                GC.Collect();
                return (int)ehReturnValue.EV_OK_ERROR;
            }
        }

        public int OnForceFieldComplete()
        {
            try
            {
                //string ValorCapturado = Buffer;

                long num5 = 0;
                string campo = EHFApp.Field.GetName().ToString();
                string valor = EHFApp.Field.GetValueStr().ToString();

                //int val = OnFieldRetyped(valor);


                bool Resultado = false;
                if (EHFApp is VerifyApp)
                {
                    if (EHFApp.Job.GetName().ToUpper().Contains("PRIMERA"))
                    {
                        #region No REQUERIDOS
                        if (campo == "cedula")
                        {
                            Resultado = ValidacaionDeCampos(campo, valor);
                            if (Resultado)
                            {

                                if (valor == "")
                                {
                                    EHFApp.Field.SetValueStr(valor);
                                    MessageBox.Show("El Campo no permite valores en blanco", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    GC.Collect();
                                    return (int)ehReturnValue.EV_OK_ABORT;
                                }
                                else
                                {

                                    #region Cedula con datos

                                    if (valor.Length < 5)
                                    {
                                        Resultado = false;
                                    }
                                    else if (valor.Length == 5)
                                    {
                                        if (valor == "00000")
                                        {
                                            Resultado = true;
                                            EHFApp.Form.GetFormField(campo, 0).SetStatus(ehFieldStatus.ehComplete);
                                        }
                                        else
                                        {
                                            Resultado = false;
                                        }
                                    }
                                    else
                                    {

                                        if (valor == "")
                                        {
                                            EHFApp.Field.SetValueStr(valor);
                                            GC.Collect(); return (int)ehReturnValue.EV_OK_ABORT;
                                        }

                                    }

                                    #endregion
                                }

                            }
                            else
                            {
                                ClassData.NumeroIntentosCaptura2++;
                                MessageBox.Show("Valor Digitado no valido", "Notificacion", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                EHFApp.Field.SetValueStr("");
                                GC.Collect();
                                return (int)ehReturnValue.EV_OK_ABORT;
                            }
                        }
                        else if (campo.ToUpper() == "ANULADO")
                        {
                            Resultado = ValidacaionDeCampos(campo, valor);
                            if (Resultado)
                            {
                                if (valor.ToUpper() == "SI")
                                {
                                    ClassData.Braile = false;
                                    ClassData.Invidente = false;
                                    ClassData.Nvlestudio = false;
                                    ClassData.Genero = false;
                                    ClassData.Capacidad = false;
                                    DataTable ObjTrazabilidad = GetDataTableBasico();
                                    string Nombre = EHFApp.Form.GetEHKey();
                                    string usuario = EHFApp.GetUserName().Trim();
                                    string Cedula = "";

                                    for (short i = 1; i <= EHFApp.Form.GetMaxNoOfFormFields(); i++)
                                    {

                                        campo = EHFApp.Form.GetFormFieldNo(i).GetName().Trim();
                                        valor = EHFApp.Form.GetFormFieldNo(i).GetValueStr().ToUpper().Trim();
                                        string estado = EHFApp.Form.GetFormFieldNo(i).GetStatus() == ehFieldStatus.ehComplete ? "1" : "0";

                                        valor = "";
                                        EHFApp.Form.GetFormFieldNo(i).SetValueStr(string.Empty);
                                        EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                                        if (campo != "Anulado")
                                        {

                                            if (campo == "Braile" || campo == "Nvlestudio" || campo == "Invidente" || campo == "Genero" || campo == "Capacidad")
                                            {

                                                EHFApp.Form.GetFormFieldNo(i).SetValueStr(string.Empty);
                                                EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                                                valor = ValidarCampos(campo, valor, ObjTrazabilidad);
                                                if (valor != "%")
                                                {
                                                    ObjTrazabilidad.Rows.Add(campo, "", "0", usuario);
                                                }
                                            }
                                            else
                                            {
                                                EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                                                // EHFApp.Form.SetStatus(ehFormStatus.ehInterpretError);
                                                ObjTrazabilidad.Rows.Add(campo, valor, estado, usuario);
                                            }
                                        }
                                        else
                                        {
                                            EHFApp.Form.GetFormField("Anulado", 0).SetValueStr(string.Empty);
                                            EHFApp.Form.GetFormField("Anulado", 0).SetStatus(ehFieldStatus.ehComplete);
                                            ObjTrazabilidad.Rows.Add(campo, "SI", "1", usuario);
                                        }

                                        //  EHFApp.Form.GetFormFieldNo(i).SetValueStr(string.Empty);

                                    }

                                    ObjTrazabilidad.AcceptChanges();
                                    ExcuteTrazabilidad(Nombre, ObjTrazabilidad, 3);

                                    EHFApp.Form.SetStatus(ehFormStatus.ehComplete);
                                    EHFApp.Form.SetQueues(2);
                                    (EHFApp as VerifyApp).SkipForm(1);
                                    GC.Collect();
                                    return (int)ehReturnValue.EV_OK;
                                }
                            }
                            else
                            {
                                ClassData.NumeroIntentosCaptura2++;
                                MessageBox.Show("Valor Digitado no valido", "Notificacion", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                EHFApp.Field.SetValueStr("");
                                GC.Collect();
                                return (int)ehReturnValue.EV_OK_ABORT;
                            }


                        }
                        else
                        {

                            Resultado = ValidacaionDeCampos(campo, valor);
                            if (Resultado)
                            {
                                #region Fecha inscripcion
                                if (campo.ToUpper().StartsWith("FI"))
                                {
                                    if (valor.Length == 2)
                                    {
                                        if (valor == "00")
                                        {
                                            EHFApp.Form.GetFormField("finsdia", 0).SetValueStr("00");
                                            EHFApp.Form.GetFormField("finsmes", 0).SetValueStr("00");
                                            EHFApp.Form.GetFormField("finsano", 0).SetValueStr("0");
                                            EHFApp.Form.GetFormField("finsdia", 0).SetStatus(ehFieldStatus.ehComplete);
                                            EHFApp.Form.GetFormField("finsmes", 0).SetStatus(ehFieldStatus.ehComplete);
                                            EHFApp.Form.GetFormField("finsano", 0).SetStatus(ehFieldStatus.ehComplete);
                                            Resultado = true;
                                        }
                                    }
                                }
                                #endregion
                                #region Fecha Expedicion
                                if (campo.ToUpper().StartsWith("FE"))
                                {
                                    if (valor.Length == 2)
                                    {
                                        if (valor == "00")
                                        {

                                            EHFApp.Form.GetFormField("fexpedia", 0).SetValueStr("00");
                                            EHFApp.Form.GetFormField("fexpemes", 0).SetValueStr("00");
                                            EHFApp.Form.GetFormField("fexpeano", 0).SetValueStr("0000");
                                            EHFApp.Form.GetFormField("fexpedia", 0).SetStatus(ehFieldStatus.ehComplete);
                                            EHFApp.Form.GetFormField("fexpemes", 0).SetStatus(ehFieldStatus.ehComplete);
                                            EHFApp.Form.GetFormField("fexpeano", 0).SetStatus(ehFieldStatus.ehComplete);
                                            Resultado = true;
                                        }
                                    }
                                }
                                #endregion

                                Resultado = ValidacaionDeCampos(campo, valor);
                                if (Resultado)
                                {
                                    ClassData.NumeroIntentosCaptura2 = 0;
                                    ClassData.VlrAprobado = "";
                                    EHFApp.Field.SetValueStr(valor);
                                    EHFApp.Field.SetStatus(ehFieldStatus.ehComplete);
                                    EHFApp.SetReturnValue(0, "Y");
                                    GC.Collect();
                                    return (int)ehReturnValue.EV_OK;
                                }
                                else
                                {
                                    ClassData.NumeroIntentosCaptura2++;
                                    MessageBox.Show("Valor Digitado no valido", "Notificacion", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    EHFApp.Field.SetValueStr("");
                                    GC.Collect();
                                    return (int)ehReturnValue.EV_OK_ABORT;
                                }
                            }
                            else
                            {
                                //ClassData.NumeroIntentosCaptura2 = 0;
                                //ClassData.VlrAprobado = "";
                                //EHFApp.Field.SetStatus(ehFieldStatus.ehComplete);
                                //EHFApp.SetReturnValue(0, "Y");
                                //GC.Collect();
                                //return (int)ehReturnValue.EV_OK;
                                ClassData.NumeroIntentosCaptura2++;
                                MessageBox.Show("Valor Digitado no valido", "Notificacion", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                EHFApp.Field.SetValueStr("");
                                GC.Collect();
                                return (int)ehReturnValue.EV_OK_ABORT;
                            }
                        }
                        #endregion
                    }
                    else if (EHFApp.Job.GetName().ToUpper().Contains("SEGUNDA"))
                    {

                        if (campo == "Nom1" || campo == "Nom2" || campo == "Ape1" || campo == "Ape2" || campo == "cedula" || campo == "fexpedia"
                            || campo == "fexpemes" || campo == "fexpeano" || campo == "finsdia" || campo == "finsmes" || campo == "finsano")
                        {
                            Resultado = ValidacaionDeCampos(campo, valor);
                            if (Resultado)
                            {
                                #region Campos Reuqeridos

                                string campoNuevo = campo;
                                string opcion = "0";
                                if (campo == "fexpedia" || campo == "fexpemes" || campo == "fexpeano")
                                {
                                    if (campo == "fexpedia")
                                    {
                                        opcion = "1";
                                    }
                                    else if (campo == "fexpemes")
                                    {
                                        opcion = "2";
                                    }
                                    else
                                    {
                                        opcion = "3";
                                    }

                                    campoNuevo = "FechaExpedicion";
                                }
                                if (campo == "finsdia" || campo == "finsmes" || campo == "finsano")
                                {
                                    if (campo == "finsdia")
                                    {
                                        opcion = "1";
                                    }
                                    else if (campo == "finsmes")
                                    {
                                        opcion = "2";
                                    }
                                    else
                                    {
                                        opcion = "3";
                                    }
                                    campoNuevo = "FechaInscripcion";
                                }
                                if (campo == "Braile")
                                {
                                    campoNuevo = "Braille";
                                }

                                #region Ventana
                                string codigoBarras = GetParametroIni("ventana");
                                if (codigoBarras == "1")
                                {
                                    string nombreDoc = EHFApp.Form.GetEHKey();
                                    Dictionary<string, object> parametros = new Dictionary<string, object>();
                                    parametros.Add("Nombre", nombreDoc);
                                    parametros.Add("campoForms", campo);
                                    parametros.Add("campo", campoNuevo);
                                    parametros.Add("opcion", opcion);
                                    DataTable dt = ExecuteSPCon(conexion, "[dbo].[Traking_Cap_Anteriores2]", parametros);
                                    if (!validarSegundaCaptura(dt, valor))
                                    {
                                        int Nintento = int.Parse(GetParametroIni("NumeroIntentos"));
                                        if (ClassData.NumeroIntentosCaptura2 >= Nintento)
                                        {
                                            MensajeCorregir obj = new MensajeCorregir();
                                            obj.ShowDialog();
                                            GC.Collect();
                                            if (ClassData.VlrAprobado != "")
                                            {
                                                ClassData.NumeroIntentosCaptura2 = 0;

                                                EHFApp.Field.SetValueStr(ClassData.VlrAprobado);
                                                ClassData.VlrAprobado = "";
                                                EHFApp.Field.SetStatus(ehFieldStatus.ehComplete);
                                                OnFieldComplete();
                                                EHFApp.SetReturnValue(0, "Y");
                                                GC.Collect();
                                                return (int)ehReturnValue.EV_OK;
                                            }
                                            else
                                            {
                                                GC.Collect();
                                                return (int)ehReturnValue.EV_OK_ABORT;
                                            }
                                        }
                                        else
                                        {
                                            ClassData.NumeroIntentosCaptura2++;
                                            EHFApp.Field.SetValueStr("");
                                            GC.Collect();
                                            return (int)ehReturnValue.EV_OK_ABORT;
                                        }

                                    }
                                }
                                #endregion
                                // }
                                #endregion
                            }
                            else
                            {
                                ClassData.NumeroIntentosCaptura2++;
                                MessageBox.Show("Valor Digitado no valido", "Notificacion", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                EHFApp.Field.SetValueStr("");
                                GC.Collect();
                                return (int)ehReturnValue.EV_OK_ABORT;
                            }
                        }
                        else if (campo.ToUpper() == "ANULADO")
                        {
                            Resultado = ValidacaionDeCampos(campo, valor);
                            if (Resultado)
                            {

                                if (valor.ToUpper() == "NO")
                                {
                                    ClassData.Braile = false;
                                    ClassData.Invidente = false;
                                    ClassData.Nvlestudio = false;
                                    ClassData.Genero = false;
                                    ClassData.Capacidad = false;
                                    DataTable ObjTrazabilidad = GetDataTableBasico();
                                    string Nombre = EHFApp.Form.GetEHKey();
                                    string usuario = EHFApp.GetUserName().Trim();
                                    string Cedula = "";

                                    for (short i = 1; i <= EHFApp.Form.GetMaxNoOfFormFields(); i++)
                                    {
                                        //if (EHFApp.Form.GetFormFieldNo(i).GetValueStr().Contains("#ErrorCaptura#"))
                                        campo = EHFApp.Form.GetFormFieldNo(i).GetName().Trim();
                                        valor = EHFApp.Form.GetFormFieldNo(i).GetValueStr().ToUpper().Trim();
                                        string estado = EHFApp.Form.GetFormFieldNo(i).GetStatus() == ehFieldStatus.ehComplete ? "1" : "0";

                                        valor = "";
                                        EHFApp.Form.GetFormFieldNo(i).SetValueStr(string.Empty);
                                        EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehRetype);


                                        if (campo != "Anulado")
                                        {

                                            if (campo == "Braile" || campo == "Nvlestudio" || campo == "Invidente" || campo == "Genero" || campo == "Capacidad")
                                            {
                                                #region Braile
                                                if (campo == "Braile")
                                                {
                                                    if (ClassData.Braile)
                                                    {
                                                        valor = "";
                                                        EHFApp.Form.GetFormFieldNo(i).SetValueStr(string.Empty);
                                                        EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                                                    }
                                                    else
                                                    {
                                                        valor = "";
                                                        EHFApp.Form.GetFormFieldNo(i).SetValueStr(string.Empty);
                                                        EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehRetype); //ClassData.Braile = true;
                                                    }
                                                }
                                                #endregion
                                                #region Nvlestudio

                                                if (campo == "Nvlestudio")
                                                {
                                                    if (ClassData.Nvlestudio)
                                                    {
                                                        valor = "";
                                                        EHFApp.Form.GetFormFieldNo(i).SetValueStr(string.Empty);
                                                        EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                                                    }
                                                    else
                                                    {
                                                        valor = "";
                                                        EHFApp.Form.GetFormFieldNo(i).SetValueStr(string.Empty);
                                                        EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehRetype); //ClassData.Nvlestudio = true;
                                                    }
                                                }
                                                #endregion
                                                #region Invidente
                                                if (campo == "Invidente")
                                                {
                                                    if (ClassData.Invidente)
                                                    {
                                                        valor = "";
                                                        EHFApp.Form.GetFormFieldNo(i).SetValueStr(string.Empty);
                                                        EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                                                    }
                                                    else
                                                    {
                                                        valor = "";
                                                        EHFApp.Form.GetFormFieldNo(i).SetValueStr(string.Empty);
                                                        EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehRetype); //ClassData.Invidente = true;
                                                    }
                                                }
                                                #endregion
                                                #region Genero
                                                if (campo == "Genero")
                                                {
                                                    if (ClassData.Genero)
                                                    {
                                                        valor = "";
                                                        EHFApp.Form.GetFormFieldNo(i).SetValueStr(string.Empty);
                                                        EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                                                    }
                                                    else
                                                    {
                                                        valor = "";
                                                        EHFApp.Form.GetFormFieldNo(i).SetValueStr(string.Empty);
                                                        EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehRetype); //ClassData.Genero = true;
                                                    }
                                                }
                                                #endregion
                                                #region Capacidad
                                                if (campo == "Capacidad")
                                                {
                                                    if (ClassData.Capacidad)
                                                    {
                                                        valor = "";
                                                        EHFApp.Form.GetFormFieldNo(i).SetValueStr(string.Empty);
                                                        EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                                                    }
                                                    else
                                                    {
                                                        valor = "";
                                                        EHFApp.Form.GetFormFieldNo(i).SetValueStr(string.Empty);
                                                        EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehRetype); //ClassData.Capacidad = true;
                                                    }
                                                }
                                                #endregion

                                            }
                                            else
                                            {
                                                EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehRetype);
                                                // EHFApp.Form.SetStatus(ehFormStatus.ehInterpretError);
                                                // ObjTrazabilidad.Rows.Add(campo, valor, estado, usuario);
                                            }
                                        }
                                        else
                                        {
                                            EHFApp.Form.GetFormField("Anulado", 0).SetValueStr(string.Empty);
                                            EHFApp.Form.GetFormField("Anulado", 0).SetStatus(ehFieldStatus.ehInterpretError);
                                            // ObjTrazabilidad.Rows.Add(campo, "SI", "1", usuario);
                                        }

                                        //  EHFApp.Form.GetFormFieldNo(i).SetValueStr(string.Empty);

                                    }


                                }
                                EHFApp.Field.SetValueStr(valor);
                                GC.Collect();
                                return (int)ehReturnValue.EV_OK;
                            }
                            else
                            {
                                ClassData.NumeroIntentosCaptura2++;
                                MessageBox.Show("Valor Digitado no valido", "Notificacion", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                EHFApp.Field.SetValueStr("");
                                GC.Collect();
                                return (int)ehReturnValue.EV_OK_ABORT;
                            }


                        }
                        else
                        {
                            #region No REQUERIDOS
                            Resultado = ValidacaionDeCampos(campo, valor);

                            if (Resultado)
                            {
                                ClassData.NumeroIntentosCaptura2 = 0;
                                ClassData.VlrAprobado = "";
                                EHFApp.Field.SetValueStr(valor);
                                EHFApp.Field.SetStatus(ehFieldStatus.ehComplete);
                                EHFApp.SetReturnValue(0, "Y");
                                GC.Collect();
                                return (int)ehReturnValue.EV_OK;
                            }
                            else
                            {
                                ClassData.NumeroIntentosCaptura2++;
                                MessageBox.Show("Valor Digitado no valido", "Notificacion", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                EHFApp.Field.SetValueStr("");
                                GC.Collect();
                                return (int)ehReturnValue.EV_OK_ABORT;
                            }
                            #endregion

                        }
                    }


                }
                EHFApp.Field.SetValueStr(valor);
                EHFApp.Field.SetStatus(ehFieldStatus.ehComplete);
                EHFApp.SetReturnValue(0, "Y");
                GC.Collect();
                return (int)ehReturnValue.EV_OK;
            }
            catch (Exception err)
            {
                Log(err.ToString());
                MessageBox.Show(err.Message + " ## " + err.InnerException != null ? err.InnerException.Message : string.Empty + " ## " + err.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                GC.Collect();
                return (int)ehReturnValue.EV_OK_ERROR;
            }
        }

        public int OnMngrAppUserDefined1()
        {
            try
            {

                SqlConnection conexion = GetConexion();
                ClassData.conexion = conexion;
                Report Reporte = new Report(ClassData.conexion);
                // Form1 Reporte = new Form1(ClassData.conexion);
                Reporte.ShowDialog();
                GC.Collect();
                return (int)ehReturnValue.EV_OK;

            }
            catch (Exception err)
            {
                Log(err.ToString());
                MessageBox.Show(err.Message + " ## " + err.InnerException != null ? err.InnerException.Message : string.Empty + " ## " + err.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                GC.Collect();
                return (int)ehReturnValue.EV_OK_ERROR;
            }
        }

        public int OnMngrAppUserDefined2()
        {
            try
            {
                Validacion Validacion = new Validacion();
                Validacion.ShowDialog();
                if (ClassData.Validacion)
                {
                    Dictionary<string, object> parametros = new Dictionary<string, object>();
                    DataTable dt = ExecuteSPCon(conexion, "[dbo].[Traking_LimpiarDatos]", parametros);
                    MessageBox.Show("Accion realizada exitosamente.");
                }

                GC.Collect();
                return (int)ehReturnValue.EV_OK;

            }
            catch (Exception err)
            {
                Log(err.ToString());
                MessageBox.Show(err.Message + " ## " + err.InnerException != null ? err.InnerException.Message : string.Empty + " ## " + err.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                GC.Collect();
                return (int)ehReturnValue.EV_OK_ERROR;
            }
        }

        public int OnMngrAppUserDefined3()
        {
            try
            {
                SqlConnection conexion = GetConexion();
                ClassData.conexion = conexion;
                Forms_Admin Reporte = new Forms_Admin(ClassData.conexion);
                Reporte.ShowDialog();
                // Form1 Reporte = new Form1(ClassData.conexion);

                GC.Collect();
                return (int)ehReturnValue.EV_OK;

            }
            catch (Exception err)
            {
                Log(err.ToString());
                MessageBox.Show(err.Message + " ## " + err.InnerException != null ? err.InnerException.Message : string.Empty + " ## " + err.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                GC.Collect();
                return (int)ehReturnValue.EV_OK_ERROR;
            }
        }

        public int OnVeriAppUserDefined1()
        {
            try
            {

                string imagen = EHFApp.Form.GetSourceImage();

                FileInfo arch = new FileInfo(imagen);
                string nombre = arch.Name.ToUpper().Replace(".TIF", "").Split('-')[0];

                Dictionary<string, object> parametros = new Dictionary<string, object>();
                parametros.Add("@NombreArchivo", nombre);
                DataTable dt = ExecuteSPCon(conexion, "[dbo].[Get_Traking_Huellas_Path]", parametros);

                if (dt.Rows.Count > 0)
                {
                    imagen = dt.Rows[0][0].ToString();
                }
                else
                {
                    string RutaOrigen = @"Z:\Repositorio\Formulario\";
                    FileInfo ar = new FileInfo(RutaOrigen + nombre + ".TIF");
                    if (ar.Exists)
                    {

                        Dictionary<string, object> parametros2 = new Dictionary<string, object>();
                        parametros2.Add("@formulario", nombre);
                        DataTable dt2 = ExecuteSPCon(conexion, "[dbo].[Get_Traking_Huellas_Path2]", parametros2);

                    }

                    Dictionary<string, object> parametros3 = new Dictionary<string, object>();
                    parametros3.Add("@NombreArchivo", nombre);
                    DataTable dt3 = ExecuteSPCon(conexion, "[dbo].[Get_Traking_Huellas_Path]", parametros3);

                    if (dt3.Rows.Count > 0)
                    {
                        imagen = dt3.Rows[0][0].ToString();
                    }
                }



                VisorFormulario Reporte = new VisorFormulario(imagen);
                // Form1 Reporte = new Form1(ClassData.conexion);
                Reporte.ShowDialog();
                GC.Collect();
                return (int)ehReturnValue.EV_OK;

            }
            catch (Exception err)
            {
                Log(err.ToString());
                MessageBox.Show(err.Message + " ## " + err.InnerException != null ? err.InnerException.Message : string.Empty + " ## " + err.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                GC.Collect();
                return (int)ehReturnValue.EV_OK_ERROR;
            }
        }

        public int OnAppStarted()
        {
            try
            {
                if (EHFApp is ManagerApp)
                {
                    ((ManagerApp)EHFApp).MngrInitUserDefinedMenu("Opciones");
                    ((ManagerApp)EHFApp).MngrAddMenuItem(1, true, "Reporte", "CONTROL+T", true);
                    ((ManagerApp)EHFApp).MngrAddMenuItem(2, true, "Limpiar Datos", "CONTROL+L", true);
                    ((ManagerApp)EHFApp).MngrAddMenuItem(3, true, "Priorizar", "CONTROL+P", true);


                }
                if (EHFApp is VerifyApp)
                {
                    ((VerifyApp)this.EHFApp).VeriInitUserDefinedMenu("Opciones");
                    ((VerifyApp)this.EHFApp).VeriAddMenuItem(1, true, "Ver Formulario", "CONTROL+F", true);
                }
                GC.Collect();
                return (int)ehReturnValue.EV_OK;
            }
            catch (Exception err)
            {
                Log(err.ToString());
                MessageBox.Show(err.Message + " ## " + err.InnerException != null ? err.InnerException.Message : string.Empty + " ## " + err.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                GC.Collect();
                return (int)ehReturnValue.EV_OK_ERROR;
            }
        }


        public int OnAppCanClose()
        {
            try
            {
                conexion.Close();
                conexion.Dispose();
                GC.Collect();
                return (int)ehReturnValue.EV_OK;
            }
            catch (Exception err)
            {
                Log(err.ToString());
                MessageBox.Show(err.Message + " ## " + err.InnerException != null ? err.InnerException.Message : string.Empty + " ## " + err.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                GC.Collect();
                return (int)ehReturnValue.EV_OK_ERROR;
            }
        }

        public int OnStopJob()
        {
            try
            {
                conexion.Close();
                conexion.Dispose();
                GC.Collect();
                return (int)ehReturnValue.EV_OK;
            }
            catch (Exception err)
            {
                Log(err.ToString());
                MessageBox.Show(err.Message + " ## " + err.InnerException != null ? err.InnerException.Message : string.Empty + " ## " + err.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                GC.Collect();
                return (int)ehReturnValue.EV_OK_ERROR;
            }
        }


        public int OnFormComplete()
        {
            try
            {

                if (EHFApp.Job.GetName().ToUpper().Contains("PRIMERA"))
                {

                    //bool Flag = true;
                    //for (short i = 1; i <= EHFApp.Form.GetMaxNoOfFormFields(); i++)
                    //{
                    //    if (EHFApp.Form.GetFormFieldNo(i).GetStatus() == ehFieldStatus.ehRetype)
                    //    {
                    //        Flag = false;
                    //    }
                    //}

                    //if (Flag)
                    //{
                    //    EHFApp.Form.SetQueues(2);
                    //}
                    //else
                    //{
                    //    EHFApp.Form.SetStatus(ehFormStatus.ehComplete);
                    //    EHFApp.Form.SetQueues(0);
                    //}

                    //(EHFApp as VerifyApp).SkipForm(1);

                }
                else if (EHFApp.Job.GetName().ToUpper().Contains("SEGUNDA"))
                {

                    bool Flag = true;
                    for (short i = 1; i <= EHFApp.Form.GetMaxNoOfFormFields(); i++)
                    {
                        //if (EHFApp.Form.GetFormFieldNo(i).GetValueStr().Contains("#ErrorCaptura#"))
                        if (EHFApp.Form.GetFormFieldNo(i).GetStatus() != ehFieldStatus.ehComplete)
                        {
                            EHFApp.Form.GetFormFieldNo(i).SetValueStr(string.Empty);
                            EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehInterpretError);
                            EHFApp.Form.SetStatus(ehFormStatus.ehInterpretError);
                            Flag = false;
                        }
                    }
                    if (!Flag)
                    {
                        EHFApp.Form.SetQueues(4);
                    }
                    else
                    {
                        EHFApp.Form.SetQueues(0);
                    }


                    (EHFApp as VerifyApp).SkipForm(1);
                }
                else
                {

                    EHFApp.Form.SetQueues(0);
                    (EHFApp as VerifyApp).SkipForm(1);
                }




                GC.Collect();
                return (int)ehReturnValue.EV_OK;
            }
            catch (Exception err)
            {
                Log(err.ToString());
                MessageBox.Show(err.Message + " ## " + err.InnerException != null ? err.InnerException.Message : string.Empty + " ## " + err.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                GC.Collect();
                return (int)ehReturnValue.EV_OK_ERROR;
            }
        }

        public int OnFormInterpreted()
        {
            try
            {
                if (EHFApp is InterpretApp)
                {
                    string nombreSet = EHFApp.Set == null ? string.Empty : EHFApp.Set.GetEHKey();
                    string setDef = EHFApp.Set == null ? string.Empty : EHFApp.Set.GetName();
                    string nombreDoc = EHFApp.Form.GetEHKey();
                    string docDef = EHFApp.Form.GetName();
                    string usuario = EHFApp.GetUserName().Trim();
                    string rutaIdent = EHFApp.Job.GetItrpImportIdentDir();
                    rutaIdent = rutaIdent.EndsWith(@"\") ? rutaIdent : rutaIdent + @"\";
                    string imagen = EHFApp.Form.GetSourceImage().Split('\\')[EHFApp.Form.GetSourceImage().Split('\\').Length - 1];
                    imagen = rutaIdent + imagen;
                    InsertIngreso(nombreSet, setDef, nombreDoc, docDef, imagen, usuario);
                }
                GC.Collect();
                return (int)ehReturnValue.EV_OK;
            }
            catch (Exception err)
            {
                Log(err.ToString());
                MessageBox.Show(err.Message + " ## " + err.InnerException != null ? err.InnerException.Message : string.Empty + " ## " + err.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                GC.Collect();
                return (int)ehReturnValue.EV_OK_ERROR;
            }
        }

        public int OnFieldRetyped(String Buffer)
        {
            try
            {


                string ValorCapturado = Buffer;

                string campo = EHFApp.Field.GetName().ToString();
                string valor = EHFApp.Field.GetValueStr().ToString();

                bool Resultado = false;
                if (EHFApp is VerifyApp)
                {
                    if (EHFApp.Job.GetName().ToUpper().Contains("PRIMERA"))
                    {

                        #region No REQUERIDOS


                        if (campo == "cedula")
                        {
                            #region cedula
                            Resultado = ValidacaionDeCampos(campo, Buffer);
                            if (Resultado)
                            {

                                if (Buffer == "")
                                {
                                    EHFApp.Field.SetValueStr(Buffer);
                                    MessageBox.Show("El Campo no permite valores en blanco", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    GC.Collect();
                                    return (int)ehReturnValue.EV_OK_ABORT;
                                }
                                else
                                {


                                    #region Cedula con datos

                                    if (Buffer.Length < 3)
                                    {
                                        Resultado = false;
                                        // EHFApp.Field.SetValueStr(Buffer);
                                        GC.Collect(); return (int)ehReturnValue.EV_OK_ABORT;
                                    }
                                    else if (Buffer.Length == 3)
                                    {
                                        //if (Buffer == "000")
                                        //{

                                        if (ValorCapturado != valor)
                                        {
                                            if (Buffer != "")
                                            {
                                                EHFApp.Field.SetValueStr(Buffer);
                                                // MessageBox.Show("El Campo: " + campo + " --No coincide--" + valor + " --- " + ValorCapturado, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                GC.Collect(); return (int)ehReturnValue.EV_OK_ABORT;
                                            }
                                        }
                                        else
                                        {
                                            Dictionary<string, object> parametros = new Dictionary<string, object>();
                                            parametros.Add("@Cedula", Buffer);
                                            parametros.Add("@Nombre", EHFApp.Form.GetEHKey());
                                            parametros.Add("@opcion", "1");
                                            DataTable dt = ExecuteSPCon(conexion, "[dbo].[Traking_Validacion_Verify]", parametros);

                                            string resultado = dt.Rows[0][0].ToString();

                                            if (resultado == "1")
                                            {
                                                EstadoBloqueo = dt.Rows[0]["Bloqueado"].ToString().ToUpper() == "0" ? false : true;
                                                EstadoNom1 = dt.Rows[0]["EstadoNom1"].ToString().ToUpper() == "0" ? false : true;
                                                EstadoNom2 = dt.Rows[0]["EstadoNom2"].ToString().ToUpper() == "0" ? false : true;
                                                EstadoApe1 = dt.Rows[0]["EstadoApe1"].ToString().ToUpper() == "0" ? false : true;
                                                EstadoApe2 = dt.Rows[0]["EstadoApe2"].ToString().ToUpper() == "0" ? false : true;
                                                EstadoFecha = dt.Rows[0]["EstadoFecha"].ToString().ToUpper() == "0" ? false : true;
                                                string Cedula = dt.Rows[0]["Cedula"].ToString();
                                                string Prinombre = dt.Rows[0]["Nombre1"].ToString();
                                                string Segnombre = dt.Rows[0]["Nombre2"].ToString();
                                                string Priapellido = dt.Rows[0]["Apellido1"].ToString();
                                                string Segapellido = dt.Rows[0]["Apellido2"].ToString();
                                                string Fexpedicion = dt.Rows[0]["Fexpedicion"].ToString();

                                                string Genero = dt.Rows[0]["Genero"].ToString().ToUpper() == "M" ? "1" : "2";
                                                string Nvlestudio = dt.Rows[0]["Nvlestudio"].ToString();
                                                string Invidente = dt.Rows[0]["Invidente"].ToString();
                                                string Braille = dt.Rows[0]["Braille"].ToString();
                                                string Email = dt.Rows[0]["Email"].ToString();
                                                string TelMovil = dt.Rows[0]["TelMovil"].ToString();
                                                string TelFijo = dt.Rows[0]["TelFijo"].ToString();
                                                string Dir = dt.Rows[0]["Dir"].ToString();
                                                string Capacidad = dt.Rows[0]["Capacidad"].ToString().ToUpper() == "S" ? "1" : "2";
                                                string FechaInscripcion = dt.Rows[0]["FechaInscripcion"].ToString();

                                                string DiaF = FechaInscripcion.Substring(0, 2);
                                                string MesF = FechaInscripcion.Substring(3, 2);
                                                string anoF = FechaInscripcion.Substring(9, 1);

                                                string Dia = Fexpedicion.Substring(0, 2);
                                                string Mes = Fexpedicion.Substring(3, 2);
                                                string ano = Fexpedicion.Substring(6, 4);

                                                #region Primer nombre
                                                EHFApp.Form.GetFormField("Nom1", 0).SetValueStr(Prinombre);
                                                if (EstadoNom1)
                                                {
                                                    EHFApp.Form.GetFormField("Nom1", 0).SetStatus(ehFieldStatus.ehComplete);
                                                }
                                                else
                                                {
                                                    EHFApp.Form.GetFormField("Nom1", 0).SetStatus(ehFieldStatus.ehValidationError);
                                                }
                                                #endregion
                                                #region Segundo nombre
                                                EHFApp.Form.GetFormField("Nom2", 0).SetValueStr(Segnombre);
                                                if (EstadoNom2)
                                                {
                                                    EHFApp.Form.GetFormField("Nom2", 0).SetStatus(ehFieldStatus.ehComplete);
                                                }
                                                else
                                                {
                                                    EHFApp.Form.GetFormField("Nom2", 0).SetStatus(ehFieldStatus.ehValidationError);
                                                }

                                                #endregion
                                                #region primer nombre
                                                EHFApp.Form.GetFormField("Ape1", 0).SetValueStr(Priapellido);
                                                if (EstadoApe1)
                                                {

                                                    EHFApp.Form.GetFormField("Ape1", 0).SetStatus(ehFieldStatus.ehComplete);
                                                }
                                                else
                                                {
                                                    EHFApp.Form.GetFormField("Ape1", 0).SetStatus(ehFieldStatus.ehValidationError);
                                                }
                                                #endregion
                                                #region segundo apellido
                                                EHFApp.Form.GetFormField("Ape2", 0).SetValueStr(Segapellido);
                                                if (EstadoApe2)
                                                {
                                                    EHFApp.Form.GetFormField("Ape2", 0).SetStatus(ehFieldStatus.ehComplete);
                                                }
                                                else
                                                {
                                                    EHFApp.Form.GetFormField("Ape2", 0).SetStatus(ehFieldStatus.ehValidationError);
                                                }
                                                #endregion
                                                #region año
                                                EHFApp.Form.GetFormField("fexpeano", 0).SetValueStr(ano);
                                                if (EstadoFecha)
                                                {
                                                    EHFApp.Form.GetFormField("fexpeano", 0).SetStatus(ehFieldStatus.ehComplete);
                                                }
                                                else
                                                {
                                                    EHFApp.Form.GetFormField("fexpeano", 0).SetStatus(ehFieldStatus.ehRetype);
                                                }
                                                #endregion
                                                #region mes
                                                EHFApp.Form.GetFormField("fexpemes", 0).SetValueStr(Mes);
                                                if (EstadoFecha)
                                                {
                                                    EHFApp.Form.GetFormField("fexpemes", 0).SetStatus(ehFieldStatus.ehComplete);
                                                }
                                                else
                                                {
                                                    EHFApp.Form.GetFormField("fexpemes", 0).SetStatus(ehFieldStatus.ehValidationError);
                                                }
                                                #endregion
                                                #region Dia
                                                EHFApp.Form.GetFormField("fexpedia", 0).SetValueStr(Dia);
                                                if (EstadoFecha)
                                                {
                                                    EHFApp.Form.GetFormField("fexpedia", 0).SetStatus(ehFieldStatus.ehComplete);
                                                }
                                                else
                                                {
                                                    EHFApp.Form.GetFormField("fexpedia", 0).SetStatus(ehFieldStatus.ehValidationError);
                                                }
                                                #endregion
                                                #region año
                                                EHFApp.Form.GetFormField("finsano", 0).SetValueStr(anoF);
                                                if (EstadoFecha)
                                                {
                                                    EHFApp.Form.GetFormField("finsano", 0).SetStatus(ehFieldStatus.ehValidationError);
                                                }
                                                else
                                                {
                                                    EHFApp.Form.GetFormField("finsano", 0).SetStatus(ehFieldStatus.ehRetype);
                                                }
                                                #endregion
                                                #region mes
                                                EHFApp.Form.GetFormField("finsmes", 0).SetValueStr(MesF);
                                                if (EstadoFecha)
                                                {
                                                    EHFApp.Form.GetFormField("finsmes", 0).SetStatus(ehFieldStatus.ehValidationError);
                                                }
                                                else
                                                {
                                                    EHFApp.Form.GetFormField("finsmes", 0).SetStatus(ehFieldStatus.ehValidationError);
                                                }
                                                #endregion
                                                #region Dia
                                                EHFApp.Form.GetFormField("finsdia", 0).SetValueStr(DiaF);
                                                if (EstadoFecha)
                                                {
                                                    EHFApp.Form.GetFormField("finsdia", 0).SetStatus(ehFieldStatus.ehValidationError);
                                                }
                                                else
                                                {
                                                    EHFApp.Form.GetFormField("finsdia", 0).SetStatus(ehFieldStatus.ehValidationError);
                                                }
                                                #endregion
                                                #region Nvlestudio

                                                EHFApp.Form.GetFormField("Nvlestudio", 1).SetValueStr(Nvlestudio);
                                                EHFApp.Form.GetFormField("Nvlestudio", 1).SetStatus(ehFieldStatus.ehRetype);
                                                EHFApp.Form.GetFormField("Nvlestudio", 2).SetValueStr("");
                                                EHFApp.Form.GetFormField("Nvlestudio", 2).SetStatus(ehFieldStatus.ehComplete);
                                                EHFApp.Form.GetFormField("Nvlestudio", 3).SetValueStr("");
                                                EHFApp.Form.GetFormField("Nvlestudio", 3).SetStatus(ehFieldStatus.ehComplete);
                                                EHFApp.Form.GetFormField("Nvlestudio", 4).SetValueStr("");
                                                EHFApp.Form.GetFormField("Nvlestudio", 4).SetStatus(ehFieldStatus.ehComplete);

                                                #endregion
                                                #region Genero
                                                try
                                                {
                                                    EHFApp.Form.GetFormField("Genero", 1).SetValueStr(Genero);
                                                    EHFApp.Form.GetFormField("Genero", 1).SetStatus(ehFieldStatus.ehRetype);
                                                    EHFApp.Form.GetFormField("Genero", 2).SetValueStr("");
                                                    EHFApp.Form.GetFormField("Genero", 2).SetStatus(ehFieldStatus.ehComplete);

                                                }
                                                catch
                                                {
                                                    EHFApp.Form.GetFormField("genero", 1).SetValueStr(Genero);
                                                    EHFApp.Form.GetFormField("genero", 1).SetStatus(ehFieldStatus.ehRetype);
                                                    EHFApp.Form.GetFormField("genero", 2).SetValueStr("");
                                                    EHFApp.Form.GetFormField("genero", 2).SetStatus(ehFieldStatus.ehComplete);
                                                }
                                                #endregion
                                                #region Capacidad

                                                EHFApp.Form.GetFormField("Capacidad", 1).SetValueStr(Capacidad);
                                                EHFApp.Form.GetFormField("Capacidad", 1).SetStatus(ehFieldStatus.ehRetype);
                                                EHFApp.Form.GetFormField("Capacidad", 2).SetValueStr("");
                                                EHFApp.Form.GetFormField("Capacidad", 2).SetStatus(ehFieldStatus.ehComplete);

                                                #endregion
                                                #region Invidente

                                                EHFApp.Form.GetFormField("Invidente", 0).SetValueStr(Invidente);
                                                EHFApp.Form.GetFormField("Invidente", 0).SetStatus(ehFieldStatus.ehRetype);


                                                #endregion
                                                #region Braille

                                                EHFApp.Form.GetFormField("Braile", 0).SetValueStr(Braille);
                                                EHFApp.Form.GetFormField("Braile", 0).SetStatus(ehFieldStatus.ehRetype);


                                                #endregion
                                                #region Email

                                                EHFApp.Form.GetFormField("Email", 0).SetValueStr(Email);
                                                EHFApp.Form.GetFormField("Email", 0).SetStatus(ehFieldStatus.ehRetype);


                                                #endregion
                                                #region TelMovil

                                                EHFApp.Form.GetFormField("TelMovil", 0).SetValueStr(TelMovil);
                                                EHFApp.Form.GetFormField("TelMovil", 0).SetStatus(ehFieldStatus.ehRetype);


                                                #endregion
                                                #region TelFijo

                                                EHFApp.Form.GetFormField("TelFijo", 0).SetValueStr(TelFijo);
                                                EHFApp.Form.GetFormField("TelFijo", 0).SetStatus(ehFieldStatus.ehRetype);


                                                #endregion
                                                #region Dir

                                                EHFApp.Form.GetFormField("Dir", 0).SetValueStr(Dir);
                                                EHFApp.Form.GetFormField("Dir", 0).SetStatus(ehFieldStatus.ehRetype);


                                                #endregion

                                                //Esta en INterpretacion
                                                EHFApp.Field.SetValueStr(Buffer);
                                                EHFApp.Field.SetStatus(ehFieldStatus.ehComplete);
                                                EHFApp.SetReturnValue(0, "Y");
                                                Captura22 = true;
                                                GC.Collect();
                                                return (int)ehReturnValue.EV_OK;

                                            }
                                            else
                                            {
                                                //Esta en ANI
                                                Dictionary<string, object> parametros2 = new Dictionary<string, object>();
                                                parametros2.Add("@Cedula", Buffer);
                                                parametros2.Add("@Nombre", EHFApp.Form.GetEHKey());
                                                parametros2.Add("@opcion", "2");
                                                DataTable dt2 = ExecuteSPCon(conexion, "[dbo].[Traking_Validacion_Verify]", parametros2);

                                                resultado = dt2.Rows[0][0].ToString();
                                                if (resultado == "1")
                                                {
                                                    Captura22 = true;
                                                    string Cedula = dt2.Rows[0]["cedula"].ToString();
                                                    string Prinombre = dt2.Rows[0]["Nombre1"].ToString();
                                                    string Segnombre = dt2.Rows[0]["Nombre2"].ToString();
                                                    string Priapellido = dt2.Rows[0]["Apellido1"].ToString();
                                                    string Segapellido = dt2.Rows[0]["Apellido2"].ToString();
                                                    string Fexpedicion = dt2.Rows[0]["Fexpedicion"].ToString();

                                                    string ano = Fexpedicion.Substring(6, 4);
                                                    string Mes = Fexpedicion.Substring(3, 2);
                                                    string Dia = Fexpedicion.Substring(0, 2);


                                                    #region Primer nombre
                                                    if (Prinombre != "%")
                                                    {
                                                        EHFApp.Form.GetFormField("Nom1", 0).SetValueStr(Prinombre);
                                                        EHFApp.Form.GetFormField("Nom1", 0).SetStatus(ehFieldStatus.ehValidationError);
                                                    }
                                                    #endregion
                                                    #region Segundo nombre
                                                    if (Segnombre != "%")
                                                    {
                                                        EHFApp.Form.GetFormField("Nom2", 0).SetValueStr(Segnombre);
                                                        EHFApp.Form.GetFormField("Nom2", 0).SetStatus(ehFieldStatus.ehValidationError);
                                                    }
                                                    #endregion
                                                    #region primer nombre
                                                    if (Priapellido != "%")
                                                    {
                                                        EHFApp.Form.GetFormField("Ape1", 0).SetValueStr(Priapellido);
                                                        EHFApp.Form.GetFormField("Ape1", 0).SetStatus(ehFieldStatus.ehValidationError);
                                                    }
                                                    #endregion
                                                    #region segundo apellido
                                                    if (Segapellido != "%")
                                                    {
                                                        EHFApp.Form.GetFormField("Ape2", 0).SetValueStr(Segapellido);
                                                        EHFApp.Form.GetFormField("Ape2", 0).SetStatus(ehFieldStatus.ehValidationError);
                                                    }
                                                    #endregion
                                                    #region año
                                                    if (ano != "%")
                                                    {
                                                        EHFApp.Form.GetFormField("fexpeano", 0).SetValueStr(ano);
                                                        EHFApp.Form.GetFormField("fexpeano", 0).SetStatus(ehFieldStatus.ehValidationError);
                                                    }
                                                    #endregion
                                                    #region mes
                                                    if (Mes != "%")
                                                    {
                                                        EHFApp.Form.GetFormField("fexpemes", 0).SetValueStr(Mes);
                                                        EHFApp.Form.GetFormField("fexpemes", 0).SetStatus(ehFieldStatus.ehValidationError);
                                                    }
                                                    #endregion
                                                    #region Dia
                                                    if (Dia != "%")
                                                    {
                                                        EHFApp.Form.GetFormField("fexpedia", 0).SetValueStr(Dia);
                                                        EHFApp.Form.GetFormField("fexpedia", 0).SetStatus(ehFieldStatus.ehValidationError);
                                                    }
                                                    #endregion
                                                }
                                                else
                                                {

                                                    EHFApp.Form.GetFormField("Nom1", 0).SetValueStr("");
                                                    EHFApp.Form.GetFormField("Nom1", 0).SetStatus(ehFieldStatus.ehValidationError);


                                                    EHFApp.Form.GetFormField("Nom2", 0).SetValueStr("");
                                                    EHFApp.Form.GetFormField("Nom2", 0).SetStatus(ehFieldStatus.ehValidationError);

                                                    EHFApp.Form.GetFormField("Ape1", 0).SetValueStr("");
                                                    EHFApp.Form.GetFormField("Ape1", 0).SetStatus(ehFieldStatus.ehValidationError);

                                                    EHFApp.Form.GetFormField("Ape2", 0).SetValueStr("");
                                                    EHFApp.Form.GetFormField("Ape2", 0).SetStatus(ehFieldStatus.ehValidationError);

                                                    EHFApp.Form.GetFormField("fexpeano", 0).SetValueStr("");
                                                    EHFApp.Form.GetFormField("fexpeano", 0).SetStatus(ehFieldStatus.ehValidationError);

                                                    EHFApp.Form.GetFormField("fexpemes", 0).SetValueStr("");
                                                    EHFApp.Form.GetFormField("fexpemes", 0).SetStatus(ehFieldStatus.ehValidationError);

                                                    EHFApp.Form.GetFormField("fexpedia", 0).SetValueStr("");
                                                    EHFApp.Form.GetFormField("fexpedia", 0).SetStatus(ehFieldStatus.ehValidationError);

                                                    #region Nvlestudio

                                                    EHFApp.Form.GetFormField("Nvlestudio", 1).SetValueStr("");
                                                    EHFApp.Form.GetFormField("Nvlestudio", 1).SetStatus(ehFieldStatus.ehValidationError);
                                                    EHFApp.Form.GetFormField("Nvlestudio", 2).SetValueStr("");
                                                    EHFApp.Form.GetFormField("Nvlestudio", 2).SetStatus(ehFieldStatus.ehComplete);
                                                    EHFApp.Form.GetFormField("Nvlestudio", 3).SetValueStr("");
                                                    EHFApp.Form.GetFormField("Nvlestudio", 3).SetStatus(ehFieldStatus.ehComplete);
                                                    EHFApp.Form.GetFormField("Nvlestudio", 4).SetValueStr("");
                                                    EHFApp.Form.GetFormField("Nvlestudio", 4).SetStatus(ehFieldStatus.ehComplete);

                                                    EHFApp.Form.GetFormField("Braile", 0).SetValueStr("");
                                                    EHFApp.Form.GetFormField("Braile", 0).SetStatus(ehFieldStatus.ehValidationError);


                                                    EHFApp.Form.GetFormField("Invidente", 0).SetValueStr("");
                                                    EHFApp.Form.GetFormField("Invidente", 0).SetStatus(ehFieldStatus.ehValidationError);

                                                    EHFApp.Form.GetFormField("Genero", 1).SetValueStr("");
                                                    EHFApp.Form.GetFormField("Genero", 1).SetStatus(ehFieldStatus.ehValidationError);
                                                    EHFApp.Form.GetFormField("Genero", 2).SetValueStr("");
                                                    EHFApp.Form.GetFormField("Genero", 2).SetStatus(ehFieldStatus.ehComplete);


                                                    EHFApp.Form.GetFormField("Capacidad", 1).SetValueStr("");
                                                    EHFApp.Form.GetFormField("Capacidad", 1).SetStatus(ehFieldStatus.ehValidationError);
                                                    EHFApp.Form.GetFormField("Capacidad", 2).SetValueStr("");
                                                    EHFApp.Form.GetFormField("Capacidad", 2).SetStatus(ehFieldStatus.ehComplete);

                                                    #endregion

                                                }
                                            }
                                        }



                                        //}
                                        //else
                                        //{
                                        //    Resultado = false;
                                        //    Resultado = false;
                                        //    // EHFApp.Field.SetValueStr(Buffer);
                                        //    GC.Collect(); return (int)ehReturnValue.EV_OK_ABORT;
                                        //}
                                    }
                                    else
                                    {

                                        if (Buffer == "")
                                        {
                                            EHFApp.Field.SetValueStr(Buffer);
                                            GC.Collect(); return (int)ehReturnValue.EV_OK_ABORT;
                                        }
                                        else if (ValorCapturado != valor)
                                        {
                                            if (Buffer != "")
                                            {
                                                EHFApp.Field.SetValueStr(Buffer);
                                                // MessageBox.Show("El Campo: " + campo + " --No coincide--" + valor + " --- " + ValorCapturado, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                GC.Collect(); return (int)ehReturnValue.EV_OK_ABORT;
                                            }

                                        }
                                        else
                                        {
                                            Dictionary<string, object> parametros = new Dictionary<string, object>();
                                            parametros.Add("@Cedula", Buffer);
                                            parametros.Add("@Nombre", EHFApp.Form.GetEHKey());
                                            parametros.Add("@opcion", "1");
                                            DataTable dt = ExecuteSPCon(conexion, "[dbo].[Traking_Validacion_Verify]", parametros);

                                            string resultado = dt.Rows[0][0].ToString();

                                            if (resultado == "1")
                                            {
                                                EstadoBloqueo = dt.Rows[0]["Bloqueado"].ToString().ToUpper() == "0" ? false : true;
                                                EstadoNom1 = dt.Rows[0]["EstadoNom1"].ToString().ToUpper() == "0" ? false : true;
                                                EstadoNom2 = dt.Rows[0]["EstadoNom2"].ToString().ToUpper() == "0" ? false : true;
                                                EstadoApe1 = dt.Rows[0]["EstadoApe1"].ToString().ToUpper() == "0" ? false : true;
                                                EstadoApe2 = dt.Rows[0]["EstadoApe2"].ToString().ToUpper() == "0" ? false : true;
                                                EstadoFecha = dt.Rows[0]["EstadoFecha"].ToString().ToUpper() == "0" ? false : true;
                                                string Cedula = dt.Rows[0]["Cedula"].ToString();
                                                string Prinombre = dt.Rows[0]["Nombre1"].ToString();
                                                string Segnombre = dt.Rows[0]["Nombre2"].ToString();
                                                string Priapellido = dt.Rows[0]["Apellido1"].ToString();
                                                string Segapellido = dt.Rows[0]["Apellido2"].ToString();
                                                string Fexpedicion = dt.Rows[0]["Fexpedicion"].ToString();

                                                string Genero = dt.Rows[0]["Genero"].ToString().ToUpper() == "M" ? "1" : "2";
                                                string Nvlestudio = dt.Rows[0]["Nvlestudio"].ToString();
                                                string Invidente = dt.Rows[0]["Invidente"].ToString();
                                                string Braille = dt.Rows[0]["Braille"].ToString();
                                                string Email = dt.Rows[0]["Email"].ToString();
                                                string TelMovil = dt.Rows[0]["TelMovil"].ToString();
                                                string TelFijo = dt.Rows[0]["TelFijo"].ToString();
                                                string Dir = dt.Rows[0]["Dir"].ToString();
                                                string Capacidad = dt.Rows[0]["Capacidad"].ToString().ToUpper() == "S" ? "1" : "2";
                                                string FechaInscripcion = dt.Rows[0]["FechaInscripcion"].ToString();

                                                string DiaF = FechaInscripcion.Substring(0, 2);
                                                string MesF = FechaInscripcion.Substring(3, 2);
                                                string anoF = FechaInscripcion.Substring(9, 1);

                                                string Dia = Fexpedicion.Substring(0, 2);
                                                string Mes = Fexpedicion.Substring(3, 2);
                                                string ano = Fexpedicion.Substring(6, 4);

                                                #region Primer nombre
                                                EHFApp.Form.GetFormField("Nom1", 0).SetValueStr(Prinombre);
                                                if (EstadoNom1)
                                                {
                                                    EHFApp.Form.GetFormField("Nom1", 0).SetStatus(ehFieldStatus.ehComplete);
                                                }
                                                else
                                                {
                                                    EHFApp.Form.GetFormField("Nom1", 0).SetStatus(ehFieldStatus.ehValidationError);
                                                }
                                                #endregion
                                                #region Segundo nombre
                                                EHFApp.Form.GetFormField("Nom2", 0).SetValueStr(Segnombre);
                                                if (EstadoNom2)
                                                {
                                                    EHFApp.Form.GetFormField("Nom2", 0).SetStatus(ehFieldStatus.ehComplete);
                                                }
                                                else
                                                {
                                                    EHFApp.Form.GetFormField("Nom2", 0).SetStatus(ehFieldStatus.ehValidationError);
                                                }

                                                #endregion
                                                #region primer nombre
                                                EHFApp.Form.GetFormField("Ape1", 0).SetValueStr(Priapellido);
                                                if (EstadoApe1)
                                                {

                                                    EHFApp.Form.GetFormField("Ape1", 0).SetStatus(ehFieldStatus.ehComplete);
                                                }
                                                else
                                                {
                                                    EHFApp.Form.GetFormField("Ape1", 0).SetStatus(ehFieldStatus.ehValidationError);
                                                }
                                                #endregion
                                                #region segundo apellido
                                                EHFApp.Form.GetFormField("Ape2", 0).SetValueStr(Segapellido);
                                                if (EstadoApe2)
                                                {
                                                    EHFApp.Form.GetFormField("Ape2", 0).SetStatus(ehFieldStatus.ehComplete);
                                                }
                                                else
                                                {
                                                    EHFApp.Form.GetFormField("Ape2", 0).SetStatus(ehFieldStatus.ehValidationError);
                                                }
                                                #endregion
                                                #region año
                                                EHFApp.Form.GetFormField("fexpeano", 0).SetValueStr(ano);
                                                if (EstadoFecha)
                                                {
                                                    EHFApp.Form.GetFormField("fexpeano", 0).SetStatus(ehFieldStatus.ehComplete);
                                                }
                                                else
                                                {
                                                    EHFApp.Form.GetFormField("fexpeano", 0).SetStatus(ehFieldStatus.ehRetype);
                                                }
                                                #endregion
                                                #region mes
                                                EHFApp.Form.GetFormField("fexpemes", 0).SetValueStr(Mes);
                                                if (EstadoFecha)
                                                {
                                                    EHFApp.Form.GetFormField("fexpemes", 0).SetStatus(ehFieldStatus.ehComplete);
                                                }
                                                else
                                                {
                                                    EHFApp.Form.GetFormField("fexpemes", 0).SetStatus(ehFieldStatus.ehValidationError);
                                                }
                                                #endregion
                                                #region Dia
                                                EHFApp.Form.GetFormField("fexpedia", 0).SetValueStr(Dia);
                                                if (EstadoFecha)
                                                {
                                                    EHFApp.Form.GetFormField("fexpedia", 0).SetStatus(ehFieldStatus.ehComplete);
                                                }
                                                else
                                                {
                                                    EHFApp.Form.GetFormField("fexpedia", 0).SetStatus(ehFieldStatus.ehValidationError);
                                                }
                                                #endregion
                                                #region año
                                                EHFApp.Form.GetFormField("finsano", 0).SetValueStr(anoF);
                                                if (EstadoFecha)
                                                {
                                                    EHFApp.Form.GetFormField("finsano", 0).SetStatus(ehFieldStatus.ehValidationError);
                                                }
                                                else
                                                {
                                                    EHFApp.Form.GetFormField("finsano", 0).SetStatus(ehFieldStatus.ehRetype);
                                                }
                                                #endregion
                                                #region mes
                                                EHFApp.Form.GetFormField("finsmes", 0).SetValueStr(MesF);
                                                if (EstadoFecha)
                                                {
                                                    EHFApp.Form.GetFormField("finsmes", 0).SetStatus(ehFieldStatus.ehValidationError);
                                                }
                                                else
                                                {
                                                    EHFApp.Form.GetFormField("finsmes", 0).SetStatus(ehFieldStatus.ehValidationError);
                                                }
                                                #endregion
                                                #region Dia
                                                EHFApp.Form.GetFormField("finsdia", 0).SetValueStr(DiaF);
                                                if (EstadoFecha)
                                                {
                                                    EHFApp.Form.GetFormField("finsdia", 0).SetStatus(ehFieldStatus.ehValidationError);
                                                }
                                                else
                                                {
                                                    EHFApp.Form.GetFormField("finsdia", 0).SetStatus(ehFieldStatus.ehValidationError);
                                                }
                                                #endregion
                                                #region Nvlestudio

                                                EHFApp.Form.GetFormField("Nvlestudio", 1).SetValueStr(Nvlestudio);
                                                EHFApp.Form.GetFormField("Nvlestudio", 1).SetStatus(ehFieldStatus.ehRetype);
                                                EHFApp.Form.GetFormField("Nvlestudio", 2).SetValueStr("");
                                                EHFApp.Form.GetFormField("Nvlestudio", 2).SetStatus(ehFieldStatus.ehComplete);
                                                EHFApp.Form.GetFormField("Nvlestudio", 3).SetValueStr("");
                                                EHFApp.Form.GetFormField("Nvlestudio", 3).SetStatus(ehFieldStatus.ehComplete);
                                                EHFApp.Form.GetFormField("Nvlestudio", 4).SetValueStr("");
                                                EHFApp.Form.GetFormField("Nvlestudio", 4).SetStatus(ehFieldStatus.ehComplete);

                                                #endregion
                                                #region Genero
                                                try
                                                {
                                                    EHFApp.Form.GetFormField("Genero", 1).SetValueStr(Genero);
                                                    EHFApp.Form.GetFormField("Genero", 1).SetStatus(ehFieldStatus.ehRetype);
                                                    EHFApp.Form.GetFormField("Genero", 2).SetValueStr("");
                                                    EHFApp.Form.GetFormField("Genero", 2).SetStatus(ehFieldStatus.ehComplete);

                                                }
                                                catch
                                                {
                                                    EHFApp.Form.GetFormField("genero", 1).SetValueStr(Genero);
                                                    EHFApp.Form.GetFormField("genero", 1).SetStatus(ehFieldStatus.ehRetype);
                                                    EHFApp.Form.GetFormField("genero", 2).SetValueStr("");
                                                    EHFApp.Form.GetFormField("genero", 2).SetStatus(ehFieldStatus.ehComplete);
                                                }
                                                #endregion
                                                #region Capacidad

                                                EHFApp.Form.GetFormField("Capacidad", 1).SetValueStr(Capacidad);
                                                EHFApp.Form.GetFormField("Capacidad", 1).SetStatus(ehFieldStatus.ehRetype);
                                                EHFApp.Form.GetFormField("Capacidad", 2).SetValueStr("");
                                                EHFApp.Form.GetFormField("Capacidad", 2).SetStatus(ehFieldStatus.ehComplete);

                                                #endregion
                                                #region Invidente

                                                EHFApp.Form.GetFormField("Invidente", 0).SetValueStr(Invidente);
                                                EHFApp.Form.GetFormField("Invidente", 0).SetStatus(ehFieldStatus.ehRetype);


                                                #endregion
                                                #region Braille

                                                EHFApp.Form.GetFormField("Braile", 0).SetValueStr(Braille);
                                                EHFApp.Form.GetFormField("Braile", 0).SetStatus(ehFieldStatus.ehRetype);


                                                #endregion
                                                #region Email

                                                EHFApp.Form.GetFormField("Email", 0).SetValueStr(Email);
                                                EHFApp.Form.GetFormField("Email", 0).SetStatus(ehFieldStatus.ehRetype);


                                                #endregion
                                                #region TelMovil

                                                EHFApp.Form.GetFormField("TelMovil", 0).SetValueStr(TelMovil);
                                                EHFApp.Form.GetFormField("TelMovil", 0).SetStatus(ehFieldStatus.ehRetype);


                                                #endregion
                                                #region TelFijo

                                                EHFApp.Form.GetFormField("TelFijo", 0).SetValueStr(TelFijo);
                                                EHFApp.Form.GetFormField("TelFijo", 0).SetStatus(ehFieldStatus.ehRetype);


                                                #endregion
                                                #region Dir

                                                EHFApp.Form.GetFormField("Dir", 0).SetValueStr(Dir);
                                                EHFApp.Form.GetFormField("Dir", 0).SetStatus(ehFieldStatus.ehRetype);


                                                #endregion

                                                //Esta en INterpretacion
                                                EHFApp.Field.SetValueStr(Buffer);
                                                EHFApp.Field.SetStatus(ehFieldStatus.ehComplete);
                                                EHFApp.SetReturnValue(0, "Y");
                                                Captura22 = true;
                                                GC.Collect();
                                                return (int)ehReturnValue.EV_OK;

                                            }
                                            else
                                            {
                                                //Esta en ANI
                                                Dictionary<string, object> parametros2 = new Dictionary<string, object>();
                                                parametros2.Add("@Cedula", Buffer);
                                                parametros2.Add("@Nombre", EHFApp.Form.GetEHKey());
                                                parametros2.Add("@opcion", "2");
                                                DataTable dt2 = ExecuteSPCon(conexion, "[dbo].[Traking_Validacion_Verify]", parametros2);

                                                resultado = dt2.Rows[0][0].ToString();
                                                if (resultado == "1")
                                                {
                                                    Captura22 = true;
                                                    string Cedula = dt2.Rows[0]["cedula"].ToString();
                                                    string Prinombre = dt2.Rows[0]["Nombre1"].ToString();
                                                    string Segnombre = dt2.Rows[0]["Nombre2"].ToString();
                                                    string Priapellido = dt2.Rows[0]["Apellido1"].ToString();
                                                    string Segapellido = dt2.Rows[0]["Apellido2"].ToString();
                                                    string Fexpedicion = dt2.Rows[0]["Fexpedicion"].ToString();


                                                    string ano ="";
                                                    string Mes ="";
                                                    string Dia = "";

                                                    if (Fexpedicion != "")
                                                    {
                                                        Fexpedicion = DateTime.Parse(Fexpedicion).ToString("dd/MM/yyyy");
                                                        ano = Fexpedicion.Substring(6, 4);
                                                        Mes = Fexpedicion.Substring(3, 2);
                                                        Dia = Fexpedicion.Substring(0, 2);
                                                       
                                                    }
                                                    

                                                    #region Primer nombre
                                                    if (Prinombre != "%")
                                                    {
                                                        EHFApp.Form.GetFormField("Nom1", 0).SetValueStr(Prinombre);
                                                        EHFApp.Form.GetFormField("Nom1", 0).SetStatus(ehFieldStatus.ehValidationError);
                                                    }
                                                    #endregion
                                                    #region Segundo nombre
                                                    if (Segnombre != "%")
                                                    {
                                                        EHFApp.Form.GetFormField("Nom2", 0).SetValueStr(Segnombre);
                                                        EHFApp.Form.GetFormField("Nom2", 0).SetStatus(ehFieldStatus.ehValidationError);
                                                    }
                                                    #endregion
                                                    #region primer nombre
                                                    if (Priapellido != "%")
                                                    {
                                                        EHFApp.Form.GetFormField("Ape1", 0).SetValueStr(Priapellido);
                                                        EHFApp.Form.GetFormField("Ape1", 0).SetStatus(ehFieldStatus.ehValidationError);
                                                    }
                                                    #endregion
                                                    #region segundo apellido
                                                    if (Segapellido != "%")
                                                    {
                                                        EHFApp.Form.GetFormField("Ape2", 0).SetValueStr(Segapellido);
                                                        EHFApp.Form.GetFormField("Ape2", 0).SetStatus(ehFieldStatus.ehValidationError);
                                                    }
                                                    #endregion
                                                    #region año
                                                    if (ano != "%")
                                                    {
                                                        EHFApp.Form.GetFormField("fexpeano", 0).SetValueStr(ano);
                                                        EHFApp.Form.GetFormField("fexpeano", 0).SetStatus(ehFieldStatus.ehValidationError);
                                                    }
                                                    #endregion
                                                    #region mes
                                                    if (Mes != "%")
                                                    {
                                                        EHFApp.Form.GetFormField("fexpemes", 0).SetValueStr(Mes);
                                                        EHFApp.Form.GetFormField("fexpemes", 0).SetStatus(ehFieldStatus.ehValidationError);
                                                    }
                                                    #endregion
                                                    #region Dia
                                                    if (Dia != "%")
                                                    {
                                                        EHFApp.Form.GetFormField("fexpedia", 0).SetValueStr(Dia);
                                                        EHFApp.Form.GetFormField("fexpedia", 0).SetStatus(ehFieldStatus.ehValidationError);
                                                    }
                                                    #endregion
                                                }
                                                else
                                                {

                                                    EHFApp.Form.GetFormField("Nom1", 0).SetValueStr("");
                                                    EHFApp.Form.GetFormField("Nom1", 0).SetStatus(ehFieldStatus.ehValidationError);


                                                    EHFApp.Form.GetFormField("Nom2", 0).SetValueStr("");
                                                    EHFApp.Form.GetFormField("Nom2", 0).SetStatus(ehFieldStatus.ehValidationError);

                                                    EHFApp.Form.GetFormField("Ape1", 0).SetValueStr("");
                                                    EHFApp.Form.GetFormField("Ape1", 0).SetStatus(ehFieldStatus.ehValidationError);

                                                    EHFApp.Form.GetFormField("Ape2", 0).SetValueStr("");
                                                    EHFApp.Form.GetFormField("Ape2", 0).SetStatus(ehFieldStatus.ehValidationError);

                                                    EHFApp.Form.GetFormField("fexpeano", 0).SetValueStr("");
                                                    EHFApp.Form.GetFormField("fexpeano", 0).SetStatus(ehFieldStatus.ehValidationError);

                                                    EHFApp.Form.GetFormField("fexpemes", 0).SetValueStr("");
                                                    EHFApp.Form.GetFormField("fexpemes", 0).SetStatus(ehFieldStatus.ehValidationError);

                                                    EHFApp.Form.GetFormField("fexpedia", 0).SetValueStr("");
                                                    EHFApp.Form.GetFormField("fexpedia", 0).SetStatus(ehFieldStatus.ehValidationError);

                                                    #region Nvlestudio

                                                    EHFApp.Form.GetFormField("Nvlestudio", 1).SetValueStr("");
                                                    EHFApp.Form.GetFormField("Nvlestudio", 1).SetStatus(ehFieldStatus.ehValidationError);
                                                    EHFApp.Form.GetFormField("Nvlestudio", 2).SetValueStr("");
                                                    EHFApp.Form.GetFormField("Nvlestudio", 2).SetStatus(ehFieldStatus.ehComplete);
                                                    EHFApp.Form.GetFormField("Nvlestudio", 3).SetValueStr("");
                                                    EHFApp.Form.GetFormField("Nvlestudio", 3).SetStatus(ehFieldStatus.ehComplete);
                                                    EHFApp.Form.GetFormField("Nvlestudio", 4).SetValueStr("");
                                                    EHFApp.Form.GetFormField("Nvlestudio", 4).SetStatus(ehFieldStatus.ehComplete);

                                                    EHFApp.Form.GetFormField("Braile", 0).SetValueStr("");
                                                    EHFApp.Form.GetFormField("Braile", 0).SetStatus(ehFieldStatus.ehValidationError);


                                                    EHFApp.Form.GetFormField("Invidente", 0).SetValueStr("");
                                                    EHFApp.Form.GetFormField("Invidente", 0).SetStatus(ehFieldStatus.ehValidationError);

                                                    EHFApp.Form.GetFormField("Genero", 1).SetValueStr("");
                                                    EHFApp.Form.GetFormField("Genero", 1).SetStatus(ehFieldStatus.ehValidationError);
                                                    EHFApp.Form.GetFormField("Genero", 2).SetValueStr("");
                                                    EHFApp.Form.GetFormField("Genero", 2).SetStatus(ehFieldStatus.ehComplete);


                                                    EHFApp.Form.GetFormField("Capacidad", 1).SetValueStr("");
                                                    EHFApp.Form.GetFormField("Capacidad", 1).SetStatus(ehFieldStatus.ehValidationError);
                                                    EHFApp.Form.GetFormField("Capacidad", 2).SetValueStr("");
                                                    EHFApp.Form.GetFormField("Capacidad", 2).SetStatus(ehFieldStatus.ehComplete);

                                                    #endregion

                                                }
                                            }
                                        }
                                    }

                                    #endregion
                                }

                            }
                            else
                            {
                                ClassData.NumeroIntentosCaptura2++;
                                MessageBox.Show("Valor Digitado no valido", "Notificacion", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                EHFApp.Field.SetValueStr("");
                                GC.Collect();
                                return (int)ehReturnValue.EV_OK_ABORT;
                            }
                            #endregion
                        }
                        else if (campo.ToUpper() == "ANULADO")
                        {
                            #region Anulado
                            Resultado = ValidacaionDeCampos(campo, Buffer);
                            if (Resultado)
                            {
                                if (Buffer.ToUpper() == "SI")
                                {
                                    ClassData.Braile = false;
                                    ClassData.Invidente = false;
                                    ClassData.Nvlestudio = false;
                                    ClassData.Genero = false;
                                    ClassData.Capacidad = false;
                                    DataTable ObjTrazabilidad = GetDataTableBasico();
                                    string Nombre = EHFApp.Form.GetEHKey();
                                    string usuario = EHFApp.GetUserName().Trim();
                                    string Cedula = "";

                                    for (short i = 1; i <= EHFApp.Form.GetMaxNoOfFormFields(); i++)
                                    {

                                        campo = EHFApp.Form.GetFormFieldNo(i).GetName().Trim();
                                        valor = EHFApp.Form.GetFormFieldNo(i).GetValueStr().ToUpper().Trim();
                                        string estado = EHFApp.Form.GetFormFieldNo(i).GetStatus() == ehFieldStatus.ehComplete ? "1" : "0";

                                        valor = "";
                                        EHFApp.Form.GetFormFieldNo(i).SetValueStr(string.Empty);
                                        EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                                        if (campo != "Anulado")
                                        {

                                            if (campo == "Braile" || campo == "Nvlestudio" || campo == "Invidente" || campo == "Genero" || campo == "Capacidad")
                                            {

                                                EHFApp.Form.GetFormFieldNo(i).SetValueStr(string.Empty);
                                                EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                                                valor = ValidarCampos(campo, valor, ObjTrazabilidad);
                                                if (valor != "%")
                                                {
                                                    ObjTrazabilidad.Rows.Add(campo, "", "0", usuario);
                                                }
                                            }
                                            else
                                            {
                                                EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                                                // EHFApp.Form.SetStatus(ehFormStatus.ehInterpretError);
                                                ObjTrazabilidad.Rows.Add(campo, valor, estado, usuario);
                                            }
                                        }
                                        else
                                        {
                                            EHFApp.Form.GetFormField("Anulado", 0).SetValueStr(string.Empty);
                                            EHFApp.Form.GetFormField("Anulado", 0).SetStatus(ehFieldStatus.ehComplete);
                                            ObjTrazabilidad.Rows.Add(campo, "SI", "1", usuario);
                                        }

                                        //  EHFApp.Form.GetFormFieldNo(i).SetValueStr(string.Empty);

                                    }

                                    ObjTrazabilidad.AcceptChanges();
                                    ExcuteTrazabilidad(Nombre, ObjTrazabilidad, 3);

                                    EHFApp.Form.SetStatus(ehFormStatus.ehComplete);
                                    EHFApp.Form.SetQueues(2);
                                    (EHFApp as VerifyApp).SkipForm(1);
                                    GC.Collect();
                                    return (int)ehReturnValue.EV_OK;
                                }
                            }
                            else
                            {
                                ClassData.NumeroIntentosCaptura2++;
                                MessageBox.Show("Valor Digitado no valido", "Notificacion", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                EHFApp.Field.SetValueStr("");
                                GC.Collect();
                                return (int)ehReturnValue.EV_OK_ABORT;
                            }
                            #endregion
                        }
                        else
                        {

                            Resultado = ValidacaionDeCampos(campo, Buffer);
                            if (!Resultado)
                            {
                                #region Fecha inscripcion
                                if (campo.ToUpper().StartsWith("FI"))
                                {
                                    if (Buffer.Length == 2)
                                    {
                                        if (Buffer == "00")
                                        {
                                            EHFApp.Form.GetFormField("finsdia", 0).SetValueStr("00");
                                            EHFApp.Form.GetFormField("finsmes", 0).SetValueStr("00");
                                            EHFApp.Form.GetFormField("finsano", 0).SetValueStr("0");
                                            EHFApp.Form.GetFormField("finsdia", 0).SetStatus(ehFieldStatus.ehComplete);
                                            EHFApp.Form.GetFormField("finsmes", 0).SetStatus(ehFieldStatus.ehComplete);
                                            EHFApp.Form.GetFormField("finsano", 0).SetStatus(ehFieldStatus.ehComplete);
                                            Resultado = true;
                                        }
                                    }
                                }
                                #endregion
                                #region Fecha Expedicion
                                if (campo.ToUpper().StartsWith("FE"))
                                {
                                    if (Buffer.Length == 2)
                                    {
                                        if (Buffer == "00")
                                        {

                                            EHFApp.Form.GetFormField("fexpedia", 0).SetValueStr("00");
                                            EHFApp.Form.GetFormField("fexpemes", 0).SetValueStr("00");
                                            EHFApp.Form.GetFormField("fexpeano", 0).SetValueStr("0000");
                                            EHFApp.Form.GetFormField("fexpedia", 0).SetStatus(ehFieldStatus.ehComplete);
                                            EHFApp.Form.GetFormField("fexpemes", 0).SetStatus(ehFieldStatus.ehComplete);
                                            EHFApp.Form.GetFormField("fexpeano", 0).SetStatus(ehFieldStatus.ehComplete);
                                            Resultado = true;
                                        }
                                    }
                                }
                                #endregion

                                if (Resultado)
                                {
                                    ClassData.NumeroIntentosCaptura2 = 0;
                                    ClassData.VlrAprobado = "";
                                    EHFApp.Field.SetValueStr(Buffer);
                                    EHFApp.Field.SetStatus(ehFieldStatus.ehComplete);
                                    EHFApp.SetReturnValue(0, "Y");
                                    GC.Collect();
                                    return (int)ehReturnValue.EV_OK;
                                }
                                else
                                {
                                    ClassData.NumeroIntentosCaptura2++;
                                    MessageBox.Show("Valor Digitado no valido", "Notificacion", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    EHFApp.Field.SetValueStr("");
                                    GC.Collect();
                                    return (int)ehReturnValue.EV_OK_ABORT;
                                }
                            }
                            else
                            {

                                ClassData.NumeroIntentosCaptura2 = 0;
                                ClassData.VlrAprobado = "";
                                if (campo == "Email")
                                { EHFApp.Field.SetValueStr(Buffer); }

                                EHFApp.Field.SetStatus(ehFieldStatus.ehComplete);
                                EHFApp.SetReturnValue(0, "Y");
                                GC.Collect();
                                return (int)ehReturnValue.EV_OK;
                            }
                        }

                        #endregion


                    }


                    else if (EHFApp.Job.GetName().ToUpper().Contains("SEGUNDA"))
                    {

                        if (campo == "cedula")
                        {
                            #region cedula
                            Resultado = ValidacaionDeCampos(campo, Buffer);
                            if (Resultado)
                            {

                                if (Buffer == "")
                                {
                                    EHFApp.Field.SetValueStr(Buffer);
                                    MessageBox.Show("El Campo no permite valores en blanco", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    GC.Collect();
                                    return (int)ehReturnValue.EV_OK_ABORT;
                                }
                                else
                                {


                                    #region Cedula con datos

                                    if (Buffer.Length < 5)
                                    {
                                        Resultado = false;
                                        // EHFApp.Field.SetValueStr(Buffer);
                                        GC.Collect(); return (int)ehReturnValue.EV_OK_ABORT;
                                    }
                                    else if (Buffer.Length == 5)
                                    {
                                        if (Buffer == "00000")
                                        {
                                            Resultado = true;
                                            EHFApp.Form.GetFormField(campo, 0).SetStatus(ehFieldStatus.ehComplete);
                                        }
                                        else
                                        {
                                            Resultado = false;
                                            Resultado = false;
                                            // EHFApp.Field.SetValueStr(Buffer);
                                            GC.Collect(); return (int)ehReturnValue.EV_OK_ABORT;
                                        }
                                    }
                                    else
                                    {
                                        if (Buffer == "")
                                        {
                                            EHFApp.Field.SetValueStr(Buffer);
                                            GC.Collect(); return (int)ehReturnValue.EV_OK_ABORT;
                                        }
                                        else if (ValorCapturado != valor)
                                        {
                                            if (Buffer != "")
                                            {
                                                EHFApp.Field.SetValueStr(Buffer);
                                                // MessageBox.Show("El Campo: " + campo + " --No coincide--" + valor + " --- " + ValorCapturado, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                GC.Collect(); return (int)ehReturnValue.EV_OK_ABORT;
                                            }

                                        }
                                        else
                                        {
                                            #region Ventana
                                            string codigoBarras = GetParametroIni("ventana");
                                            if (codigoBarras == "1")
                                            {
                                                string nombreDoc = EHFApp.Form.GetEHKey();
                                                Dictionary<string, object> parametros = new Dictionary<string, object>();
                                                parametros.Add("Nombre", nombreDoc);
                                                parametros.Add("campoForms", campo);
                                                parametros.Add("campo", campo);
                                                parametros.Add("opcion", "0");
                                                DataTable dt = ExecuteSPCon(conexion, "[dbo].[Traking_Cap_Anteriores2]", parametros);
                                                if (!validarSegundaCaptura(dt, valor))
                                                {
                                                    int Nintento = int.Parse(GetParametroIni("NumeroIntentos"));
                                                    if (ClassData.NumeroIntentosCaptura2 >= Nintento)
                                                    {
                                                        MensajeCorregir obj = new MensajeCorregir();
                                                        obj.ShowDialog();
                                                        GC.Collect();
                                                        if (ClassData.VlrAprobado != "")
                                                        {
                                                            ClassData.NumeroIntentosCaptura2 = 0;

                                                            EHFApp.Field.SetValueStr(ClassData.VlrAprobado);
                                                            ClassData.VlrAprobado = "";
                                                            EHFApp.Field.SetStatus(ehFieldStatus.ehComplete);
                                                            OnFieldComplete();
                                                            EHFApp.SetReturnValue(0, "Y");
                                                            GC.Collect();
                                                            return (int)ehReturnValue.EV_OK;
                                                        }
                                                        else
                                                        {
                                                            GC.Collect();
                                                            return (int)ehReturnValue.EV_OK_ABORT;
                                                        }
                                                    }
                                                    else if (campo == "cedula")
                                                    {
                                                        MensajeCorregir obj = new MensajeCorregir();
                                                        obj.ShowDialog();
                                                        GC.Collect();
                                                        if (ClassData.VlrAprobado != "")
                                                        {
                                                            ClassData.NumeroIntentosCaptura2 = 0;

                                                            EHFApp.Field.SetValueStr(ClassData.VlrAprobado);
                                                            ClassData.VlrAprobado = "";
                                                            EHFApp.Field.SetStatus(ehFieldStatus.ehComplete);
                                                            OnFieldComplete();
                                                            EHFApp.SetReturnValue(0, "Y");
                                                            GC.Collect();
                                                            return (int)ehReturnValue.EV_OK;
                                                        }
                                                        else
                                                        {
                                                            GC.Collect();
                                                            return (int)ehReturnValue.EV_OK_ABORT;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        ClassData.NumeroIntentosCaptura2++;
                                                        EHFApp.Field.SetValueStr("");
                                                        GC.Collect();
                                                        return (int)ehReturnValue.EV_OK_ABORT;
                                                    }

                                                }
                                            }
                                            #endregion
                                        }
                                    }

                                    #endregion
                                }

                            }
                            else
                            {
                                ClassData.NumeroIntentosCaptura2++;
                                MessageBox.Show("Valor Digitado no valido", "Notificacion", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                EHFApp.Field.SetValueStr("");
                                GC.Collect();
                                return (int)ehReturnValue.EV_OK_ABORT;
                            }
                            #endregion
                        }
                        else if (campo.ToUpper() == "ANULADO")
                        {
                            Resultado = ValidacaionDeCampos(campo, Buffer);
                            if (Resultado)
                            {

                                if (Buffer.ToUpper() == "SI")
                                {
                                    ClassData.Braile = false;
                                    ClassData.Invidente = false;
                                    ClassData.Nvlestudio = false;
                                    ClassData.Genero = false;
                                    ClassData.Capacidad = false;
                                    DataTable ObjTrazabilidad = GetDataTableBasico();
                                    string Nombre = EHFApp.Form.GetEHKey();
                                    string usuario = EHFApp.GetUserName().Trim();
                                    string Cedula = "";

                                    for (short i = 1; i <= EHFApp.Form.GetMaxNoOfFormFields(); i++)
                                    {
                                        //if (EHFApp.Form.GetFormFieldNo(i).GetValueStr().Contains("#ErrorCaptura#"))
                                        campo = EHFApp.Form.GetFormFieldNo(i).GetName().Trim();
                                        valor = EHFApp.Form.GetFormFieldNo(i).GetValueStr().ToUpper().Trim();
                                        string estado = EHFApp.Form.GetFormFieldNo(i).GetStatus() == ehFieldStatus.ehComplete ? "1" : "0";

                                        valor = "";
                                        if (campo != "Anulado")
                                        {
                                            EHFApp.Form.GetFormFieldNo(i).SetValueStr(string.Empty);
                                        }
                                        EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);

                                        //  EHFApp.Form.GetFormFieldNo(i).SetValueStr(string.Empty);

                                    }
                                }
                                EHFApp.Field.SetValueStr(Buffer);
                                GC.Collect();
                                return (int)ehReturnValue.EV_OK;
                            }
                            else
                            {
                                ClassData.NumeroIntentosCaptura2++;
                                MessageBox.Show("Valor Digitado no valido", "Notificacion", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                EHFApp.Field.SetValueStr("");
                                GC.Collect();
                                return (int)ehReturnValue.EV_OK_ABORT;
                            }


                        }
                        else
                        {
                            #region No REQUERIDOS
                            Resultado = ValidacaionDeCampos(campo, Buffer);

                            if (Resultado)
                            {

                                if (campo == "Nom1" || campo == "Nom2" || campo == "Ape1" || campo == "Ape2" || campo == "cedula" || campo == "fexpedia"
                                 || campo == "fexpemes" || campo == "fexpeano" || campo == "finsdia" || campo == "finsmes" || campo == "finsano")
                                {


                                    #region Ventana


                                    string codigoBarras = GetParametroIni("ventana");
                                    if (codigoBarras == "1")
                                    {
                                        string nombreDoc = EHFApp.Form.GetEHKey();
                                        Dictionary<string, object> parametros = new Dictionary<string, object>();
                                        parametros.Add("Nombre", nombreDoc);
                                        parametros.Add("campoForms", campo);
                                        parametros.Add("campo", campo);
                                        parametros.Add("opcion", "0");
                                        DataTable dt = ExecuteSPCon(conexion, "[dbo].[Traking_Cap_Anteriores2]", parametros);
                                        if (!validarSegundaCaptura(dt, Buffer))
                                        {
                                            int Nintento = int.Parse(GetParametroIni("NumeroIntentos"));
                                            if (ClassData.NumeroIntentosCaptura2 >= Nintento)
                                            {
                                                MensajeCorregir obj = new MensajeCorregir();
                                                obj.ShowDialog();
                                                GC.Collect();
                                                if (ClassData.VlrAprobado != "")
                                                {


                                                    ClassData.NumeroIntentosCaptura2 = 0;

                                                    EHFApp.Field.SetValueStr(ClassData.VlrAprobado);
                                                    ClassData.VlrAprobado = "";
                                                    EHFApp.Field.SetStatus(ehFieldStatus.ehComplete);
                                                    // OnFieldComplete();
                                                    EHFApp.SetReturnValue(0, "Y");
                                                    GC.Collect();
                                                    return (int)ehReturnValue.EV_OK;
                                                }
                                                else
                                                {
                                                    GC.Collect();
                                                    return (int)ehReturnValue.EV_OK_ABORT;
                                                }
                                            }
                                            else
                                            {
                                                ClassData.NumeroIntentosCaptura2++;
                                                EHFApp.Field.SetValueStr("");
                                                GC.Collect();
                                                return (int)ehReturnValue.EV_OK_ABORT;
                                            }

                                        }
                                        else
                                        {
                                            ClassData.NumeroIntentosCaptura2++;
                                            //  MessageBox.Show("Valor Digitado no valido", "Notificacion", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                            EHFApp.Field.SetValueStr(Buffer);
                                            GC.Collect();
                                            return (int)ehReturnValue.EV_OK;
                                        }
                                    }
                                    #endregion

                                }

                                ClassData.NumeroIntentosCaptura2 = 0;
                                ClassData.VlrAprobado = "";
                                EHFApp.Field.SetValueStr(Buffer);
                                EHFApp.Field.SetStatus(ehFieldStatus.ehComplete);
                                EHFApp.SetReturnValue(0, "Y");
                                GC.Collect();
                                return (int)ehReturnValue.EV_OK;
                            }
                            else
                            {
                                ClassData.NumeroIntentosCaptura2++;
                                MessageBox.Show("Valor Digitado no valido", "Notificacion", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                EHFApp.Field.SetValueStr("");
                                GC.Collect();
                                return (int)ehReturnValue.EV_OK_ABORT;
                            }
                            #endregion

                        }
                    }
                }

                EHFApp.Field.SetValueStr(Buffer);
                EHFApp.Field.SetStatus(ehFieldStatus.ehComplete);
                EHFApp.SetReturnValue(0, "Y");
                GC.Collect();
                return (int)ehReturnValue.EV_OK;

            }
            catch (Exception err)
            {
                Log(err.ToString());
                MessageBox.Show(err.Message + " ## " + err.InnerException != null ? err.InnerException.Message : string.Empty + " ## " + err.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                GC.Collect();
                return (int)ehReturnValue.EV_OK_ERROR;
            }
        }

        public int OnFieldValidateNonExisting()
        {
            try
            {
                if (EHFApp.Form.GetName().StartsWith("E"))
                {
                    GC.Collect();
                    return (int)ehReturnValue.EV_OK;
                }

                if (EHFApp.Field.GetValueStr().ToUpper() == string.Empty)
                {
                    EHFApp.Field.SetStatus(ehFieldStatus.ehInterpretError);
                }
                GC.Collect();
                return (int)ehReturnValue.EV_OK;
            }
            catch (Exception err)
            {
                Log(err.ToString());
                MessageBox.Show(err.Message + " ## " + err.InnerException != null ? err.InnerException.Message : string.Empty + " ## " + err.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                GC.Collect();
                return (int)ehReturnValue.EV_OK_ERROR;
            }
        }

        public int OnFieldVerify()
        {
            try
            {
                int Resultado = 0;
                long num5;
                string campo = EHFApp.Field.GetName().ToString();
                string valor = EHFApp.Field.GetValueStr().ToString();

                if (campo != ClassData.UsuarioNumeroIntentosCaptura2)
                {
                    //   EHFApp.Field.SetValueStr(valor);
                    if (EHFApp.Job.GetName().ToUpper().Contains("PRIMERA"))
                    {
                        if (!campo.ToUpper().StartsWith("FI"))
                        {
                            if (campo != "Nom1" && campo != "Nom2" && campo != "Ape1" && campo != "Ape2")
                            {
                                if (campo == "TelFijo" || campo == "TelMovil")
                                {
                                    if (valor == "0")
                                        valor = "";

                                    OnFieldRetyped(valor);
                                }
                                else if (valor != "")
                                {
                                    OnFieldRetyped(valor);
                                }
                                else if (campo == "Nom2")
                                {
                                    OnFieldRetyped(valor);
                                }

                            }

                        }

                    }
                    ClassData.UsuarioNumeroIntentosCaptura2 = campo;
                    ClassData.NumeroIntentosCaptura2 = 0;
                }
                if (EHFApp.Job.GetName().ToUpper().Contains("SEGUNDA"))
                {
                    if (campo.ToUpper() == "ANULADO")
                    {

                        bool Resultad = ValidacaionDeCampos(campo, valor);
                        if (Resultad)
                        {

                            if (valor.ToUpper() == "SI")
                            {
                                ClassData.Braile = false;
                                ClassData.Invidente = false;
                                ClassData.Nvlestudio = false;
                                ClassData.Genero = false;
                                ClassData.Capacidad = false;
                                DataTable ObjTrazabilidad = GetDataTableBasico();
                                string Nombre = EHFApp.Form.GetEHKey();
                                string usuario = EHFApp.GetUserName().Trim();
                                string Cedula = "";

                                for (short i = 1; i <= EHFApp.Form.GetMaxNoOfFormFields(); i++)
                                {
                                    //if (EHFApp.Form.GetFormFieldNo(i).GetValueStr().Contains("#ErrorCaptura#"))
                                    campo = EHFApp.Form.GetFormFieldNo(i).GetName().Trim();
                                    valor = EHFApp.Form.GetFormFieldNo(i).GetValueStr().ToUpper().Trim();
                                    string estado = EHFApp.Form.GetFormFieldNo(i).GetStatus() == ehFieldStatus.ehComplete ? "1" : "0";

                                    valor = "";
                                    EHFApp.Form.GetFormFieldNo(i).SetValueStr(string.Empty);
                                    EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehRetype);


                                    if (campo != "Anulado")
                                    {

                                        if (campo == "Braile" || campo == "Nvlestudio" || campo == "Invidente" || campo == "Genero" || campo == "Capacidad")
                                        {
                                            #region Braile
                                            if (campo == "Braile")
                                            {
                                                if (ClassData.Braile)
                                                {
                                                    valor = "";
                                                    EHFApp.Form.GetFormFieldNo(i).SetValueStr(string.Empty);
                                                    EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                                                }
                                                else
                                                {
                                                    valor = "";
                                                    EHFApp.Form.GetFormFieldNo(i).SetValueStr(string.Empty);
                                                    EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehRetype); //ClassData.Braile = true;
                                                }
                                            }
                                            #endregion
                                            #region Nvlestudio

                                            if (campo == "Nvlestudio")
                                            {
                                                if (ClassData.Nvlestudio)
                                                {
                                                    valor = "";
                                                    EHFApp.Form.GetFormFieldNo(i).SetValueStr(string.Empty);
                                                    EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                                                }
                                                else
                                                {
                                                    valor = "";
                                                    EHFApp.Form.GetFormFieldNo(i).SetValueStr(string.Empty);
                                                    EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehRetype); //ClassData.Nvlestudio = true;
                                                }
                                            }
                                            #endregion
                                            #region Invidente
                                            if (campo == "Invidente")
                                            {
                                                if (ClassData.Invidente)
                                                {
                                                    valor = "";
                                                    EHFApp.Form.GetFormFieldNo(i).SetValueStr(string.Empty);
                                                    EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                                                }
                                                else
                                                {
                                                    valor = "";
                                                    EHFApp.Form.GetFormFieldNo(i).SetValueStr(string.Empty);
                                                    EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehRetype); //ClassData.Invidente = true;
                                                }
                                            }
                                            #endregion
                                            #region Genero
                                            if (campo == "Genero")
                                            {
                                                if (ClassData.Genero)
                                                {
                                                    valor = "";
                                                    EHFApp.Form.GetFormFieldNo(i).SetValueStr(string.Empty);
                                                    EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                                                }
                                                else
                                                {
                                                    valor = "";
                                                    EHFApp.Form.GetFormFieldNo(i).SetValueStr(string.Empty);
                                                    EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehRetype); //ClassData.Genero = true;
                                                }
                                            }
                                            #endregion
                                            #region Capacidad
                                            if (campo == "Capacidad")
                                            {
                                                if (ClassData.Capacidad)
                                                {
                                                    valor = "";
                                                    EHFApp.Form.GetFormFieldNo(i).SetValueStr(string.Empty);
                                                    EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                                                }
                                                else
                                                {
                                                    valor = "";
                                                    EHFApp.Form.GetFormFieldNo(i).SetValueStr(string.Empty);
                                                    EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehRetype); //ClassData.Capacidad = true;
                                                }
                                            }
                                            #endregion

                                        }
                                        else
                                        {
                                            EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehRetype);
                                            // EHFApp.Form.SetStatus(ehFormStatus.ehInterpretError);
                                            // ObjTrazabilidad.Rows.Add(campo, valor, estado, usuario);
                                        }
                                    }
                                    else
                                    {
                                        EHFApp.Form.GetFormField("Anulado", 0).SetValueStr(string.Empty);
                                        EHFApp.Form.GetFormField("Anulado", 0).SetStatus(ehFieldStatus.ehInterpretError);
                                        // ObjTrazabilidad.Rows.Add(campo, "SI", "1", usuario);
                                    }

                                    //  EHFApp.Form.GetFormFieldNo(i).SetValueStr(string.Empty);

                                }


                            }
                            else if (valor.ToUpper() == "SI")
                            {
                                EHFApp.Field.SetValueStr(valor);
                                GC.Collect();
                                return (int)ehReturnValue.EV_OK;
                            }
                            else
                            {

                            }

                        }
                        else
                        {
                            //ClassData.NumeroIntentosCaptura2++;
                            //MessageBox.Show("Valor Digitado no valido", "Notificacion", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            //EHFApp.Field.SetValueStr("");
                            //GC.Collect();
                            //return (int)ehReturnValue.EV_OK_ABORT;
                        }
                    }
                    else
                    {
                        if (campo == "Nvlestudio")
                        {
                             
                                EHFApp.Form.GetFormField("Nvlestudio", 2).SetValueStr(string.Empty);
                                EHFApp.Form.GetFormField("Nvlestudio", 2).SetStatus(ehFieldStatus.ehComplete);
                                EHFApp.Form.GetFormField("Nvlestudio", 3).SetValueStr(string.Empty);
                                EHFApp.Form.GetFormField("Nvlestudio", 3).SetStatus(ehFieldStatus.ehComplete);
                                EHFApp.Form.GetFormField("Nvlestudio", 4).SetValueStr(string.Empty);
                                EHFApp.Form.GetFormField("Nvlestudio", 4).SetStatus(ehFieldStatus.ehComplete);

                            
                        }
                        if (campo == "Genero")
                        {

                            EHFApp.Form.GetFormField("Genero", 2).SetValueStr(string.Empty);
                            EHFApp.Form.GetFormField("Genero", 2).SetStatus(ehFieldStatus.ehComplete);
                        }
                        if (campo == "Capacidad")
                        {

                            EHFApp.Form.GetFormField("Capacidad", 2).SetValueStr(string.Empty);
                            EHFApp.Form.GetFormField("Capacidad", 2).SetStatus(ehFieldStatus.ehComplete);

                        }

                        //MOMENTANIAMENTE REMOVIDO
                        //if (campo == "TelMovil" || campo == "finsdia")
                        //{
                        //    if (EHFApp.Field.GetStatus() != ehFieldStatus.ehComplete)
                        //    {

                        //       // EHFApp.Field.SetStatus(ehFieldStatus.ehValidationError);
                        //        EHFApp.Field.SetStatus(ehFieldStatus.ehRetype);

                        //        bool Resultado_ = false;
                        //        Resultado_ = ValidacaionDeCampos(campo, valor);
                        //        if (Resultado_)
                        //        {
                        //            GC.Collect();
                        //            return (int)ehReturnValue.EV_OK;
                        //        }
                        //        else
                        //        {
                        //            GC.Collect();
                        //            return (int)ehReturnValue.EV_OK;
                        //        }
                        //    }
                        //}

                    }
                }
                else
                {
                    if (campo.ToUpper() == "ANULADO")
                    {
                        if (valor.ToUpper() == "SI")
                        {
                            // EHFApp.Field.SetValueStr("");
                            EHFApp.Field.SetStatus(ehFieldStatus.ehRetype);
                            EHFApp.SetReturnValue(0, "Y");
                            GC.Collect();
                            return (int)ehReturnValue.EV_OK_ABORT;

                        }
                    }
                    else
                    {
                        if (campo.ToUpper().StartsWith("FI"))
                        {
                            if (valor.Length == 2)
                            {
                                if (valor == "00")
                                {
                                    EHFApp.Form.GetFormField("finsdia", 0).SetValueStr("00");
                                    EHFApp.Form.GetFormField("finsmes", 0).SetValueStr("00");
                                    EHFApp.Form.GetFormField("finsano", 0).SetValueStr("0");
                                    EHFApp.Form.GetFormField("finsdia", 0).SetStatus(ehFieldStatus.ehComplete);
                                    EHFApp.Form.GetFormField("finsmes", 0).SetStatus(ehFieldStatus.ehComplete);
                                    EHFApp.Form.GetFormField("finsano", 0).SetStatus(ehFieldStatus.ehComplete);

                                }
                            }
                        }
                        if (campo.ToUpper().StartsWith("FE"))
                        {
                            if (valor.Length == 2)
                            {
                                if (valor == "00")
                                {

                                    EHFApp.Form.GetFormField("fexpedia", 0).SetValueStr("00");
                                    EHFApp.Form.GetFormField("fexpemes", 0).SetValueStr("00");
                                    EHFApp.Form.GetFormField("fexpeano", 0).SetValueStr("0000");
                                    EHFApp.Form.GetFormField("fexpedia", 0).SetStatus(ehFieldStatus.ehComplete);
                                    EHFApp.Form.GetFormField("fexpemes", 0).SetStatus(ehFieldStatus.ehComplete);
                                    EHFApp.Form.GetFormField("fexpeano", 0).SetStatus(ehFieldStatus.ehComplete);

                                }
                            }
                        }


                    }
                }

                GC.Collect();
                return (int)ehReturnValue.EV_OK;
            }
            catch
            {
                GC.Collect();
                return (int)ehReturnValue.EV_OK_ERROR;
            }
        }

        public int OnFormValidated()
        {
            try
            {
                int proceso = 1;
                string toVerify = GetParametroIni("toVerify");
                InsertTrazabilidad(proceso, toVerify == "0");
                GC.Collect();
                return (int)ehReturnValue.EV_OK;
            }
            catch (Exception err)
            {
                Log(err.ToString());
                MessageBox.Show(err.Message + " ## " + err.InnerException != null ? err.InnerException.Message : string.Empty + " ## " + err.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                GC.Collect();
                return (int)ehReturnValue.EV_OK_ERROR;
            }
        }

        public int OnFormValidate()
        {
            try
            {
                int proceso = 2;

                if (EHFApp.Job.GetName().ToUpper().Contains("PRIMERA"))
                {
                    proceso = 2;
                }
                else if (EHFApp.Job.GetName().ToUpper().Contains("SEGUNDA"))
                {
                    proceso = 3;
                }


                InsertTrazabilidad(proceso);
                GC.Collect();
                return (int)ehReturnValue.EV_OK;
            }
            catch (Exception err)
            {
                Log(err.ToString());
                MessageBox.Show(err.Message + " ## " + err.InnerException != null ? err.InnerException.Message : string.Empty + " ## " + err.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                GC.Collect();
                return (int)ehReturnValue.EV_OK_ERROR;
            }
        }

        public int OnFormTransferEnd()
        {
            try
            {
                string nombreDoc = EHFApp.Form.GetEHKey();
                string usuario = EHFApp.GetUserName().Trim();
                string imagen = EHFApp.Form.GetSourceImage();
                if (EHFApp is TransferApp)
                {
                    if (!EHFApp.Job.GetName().ToUpper().Contains("PRIMERA") && !EHFApp.Job.GetName().ToUpper().Contains("SEGUNDA"))
                    {
                        if (EHFApp.Job.GetName().ToUpper().Contains("CONSULA"))
                        {
                            bool EstaBien = false;
                            Dictionary<string, object> parametros = new Dictionary<string, object>();
                            parametros.Add("@Nombre", nombreDoc);
                            DataTable dt = ExecuteSPCon(conexion, "[dbo].[Traking_Transfer_Formulario_Consulado]", parametros);
                            if (dt.Rows.Count > 0)
                            {


                                #region Variables
                                String NombreFormulario = dt.Rows[0][0].ToString();
                                String RutaFormulario = dt.Rows[0][1].ToString();
                                FileInfo Archivo = new FileInfo(imagen);
                                string HexaFormulario = Archivo.Name.Replace(".TIF", "").ToString();

                                FileInfo ArchivoCp1 = new FileInfo(Archivo.DirectoryName + "\\" + HexaFormulario + "_Cupon1.BMP");
                                if (!ArchivoCp1.Exists)
                                {
                                    string DosUltimos = HexaFormulario.Substring(HexaFormulario.Length - 2, 2);
                                    String NuevoDosUltimo = (int.Parse(DosUltimos) - 2).ToString().PadLeft(2, '0');
                                    String Comienzo = HexaFormulario.Substring(0, HexaFormulario.Length - 2);
                                    Comienzo = Comienzo + NuevoDosUltimo;
                                    FileInfo Archivo2Cp1 = new FileInfo(Archivo.DirectoryName + "\\" + Comienzo + "_Cupon1.BMP");
                                    if (Archivo2Cp1.Exists)
                                    {
                                        Archivo = new FileInfo(Archivo.DirectoryName + "\\" + Comienzo + ".TIF");
                                        EstaBien = true;
                                    }
                                }
                                else
                                {
                                    EstaBien = true;
                                }

                                if (EstaBien)
                                {
                                    string Directory = Archivo.Directory.ToString() + "\\";
                                    string Cupon1 = Archivo.Name.Replace(".TIF", "_Cupon1.BMP");
                                    string Cupon2 = Archivo.Name.Replace(".TIF", "_Cupon2.BMP");
                                    string Cupon3 = Archivo.Name.Replace(".TIF", "_Cupon3.BMP");

                                    #endregion

                                    #region Renombra Archivo
                                    DirectoryInfo Dir = new DirectoryInfo(Directory + "Cupon\\");
                                    if (!Dir.Exists) { Dir.Create(); }

                                    DirectoryInfo Form = new DirectoryInfo(Directory + "Formulario\\");
                                    if (!Form.Exists) { Form.Create(); }

                                    string Directory2 = Directory + "Cupon\\";
                                    GuardarImagenTiff(300, Directory + Cupon1, RutaFormulario + NombreFormulario + "-Cupon1.Tif");
                                    GuardarImagenTiff(300, Directory + Cupon2, RutaFormulario + NombreFormulario + "-Cupon2.Tif");
                                    GuardarImagenTiff(300, Directory + Cupon3, RutaFormulario + NombreFormulario + "-Cupon3.Tif");

                                    File.Copy(RutaFormulario + NombreFormulario + "-Cupon1.Tif", Directory2 + NombreFormulario + "-Cupon1.Tif", true);
                                    File.Copy(RutaFormulario + NombreFormulario + "-Cupon2.Tif", Directory2 + NombreFormulario + "-Cupon2.Tif", true);
                                    File.Copy(RutaFormulario + NombreFormulario + "-Cupon3.Tif", Directory2 + NombreFormulario + "-Cupon3.Tif", true);

                                    FileInfo a = new FileInfo(Directory2 + NombreFormulario + "-Cupon1.BMP");
                                    if (a.Exists)
                                    {
                                        File.Move(Directory2 + NombreFormulario + "-Cupon1.BMP", Directory2 + NombreFormulario + "-Cupon1.BMP".ToUpper().ToString().Replace(".BMP", "_1.BMP"));
                                    }
                                    FileInfo b = new FileInfo(Directory2 + NombreFormulario + "-Cupon2.BMP");
                                    if (b.Exists)
                                    {
                                        File.Move(Directory2 + NombreFormulario + "-Cupon2.BMP", Directory2 + NombreFormulario + "-Cupon2.BMP".ToUpper().ToString().Replace(".BMP", "_1.BMP"));
                                        b.Delete();
                                    }

                                    FileInfo c = new FileInfo(Directory2 + NombreFormulario + "-Cupon3.BMP");
                                    if (c.Exists)
                                    {
                                        File.Move(Directory2 + NombreFormulario + "-Cupon3.BMP", Directory2 + NombreFormulario + "-Cupon3.BMP".ToUpper().ToString().Replace(".BMP", "_1.BMP"));
                                        c.Delete();
                                    }

                                    FileInfo d = new FileInfo(Directory + "Formulario\\" + NombreFormulario + ".TIF");
                                    if (d.Exists)
                                    {
                                        File.Move(Directory + "Formulario\\" + NombreFormulario + ".TIF", Directory + "Formulario\\" + NombreFormulario + ".TIF".ToUpper().ToString().Replace(".BMP", "_1.BMP"));
                                        d.Delete();
                                    }

                                    File.Move(Directory + Cupon1, Directory2 + NombreFormulario + "-Cupon1.BMP");
                                    File.Move(Directory + Cupon2, Directory2 + NombreFormulario + "-Cupon2.BMP");
                                    File.Move(Directory + Cupon3, Directory2 + NombreFormulario + "-Cupon3.BMP");


                                    File.Move(Directory + HexaFormulario + ".TIF", Directory + "Formulario\\" + NombreFormulario + ".TIF");

                                    #endregion

                                    //#region Recorte Huella

                                    //Recorte(Directory2 + NombreFormulario + "-Cupon1.BMP", 500);
                                    //BinaryImage(Directory + "Recorte\\" + NombreFormulario + "-Recorte1.BMP");

                                    //Recorte(Directory2 + NombreFormulario + "-Cupon2.BMP", 500);
                                    //BinaryImage(Directory + "Recorte\\" + NombreFormulario + "-Recorte2.BMP");


                                    //Recorte(Directory2 + NombreFormulario + "-Cupon3.BMP", 500);
                                    //BinaryImage(Directory + "Recorte\\" + NombreFormulario + "-Recorte3.BMP");


                                    //#endregion

                                    //File.Delete(Directory2 + NombreFormulario + "_Cupon1.BMP");
                                    //File.Delete(Directory2 + NombreFormulario + "_Cupon2.BMP");
                                    //File.Delete(Directory2 + NombreFormulario + "_Cupon3.BMP");

                                    #region trazabilidad
                                    InsertRecorte("Cupon", Directory + "Cupon\\" + NombreFormulario + "_Cupon1.Fif", NombreFormulario, NombreFormulario + "_Cupon1");
                                    //InsertRecorte("Huella", Directory + "Recorte\\Huella\\" + NombreFormulario + "_Huella1.BMP", NombreFormulario, NombreFormulario + "_Cupon1");

                                    InsertRecorte("Cupon", Directory + "Cupon\\" + NombreFormulario + "_Cupon2.Tif", NombreFormulario, NombreFormulario + "_Cupon2");
                                    //InsertRecorte("Huella", Directory + "Recorte\\Huella\\" + NombreFormulario + "_Huella2.BMP", NombreFormulario, NombreFormulario + "_Cupon2");

                                    InsertRecorte("Cupon", Directory + "Cupon\\" + NombreFormulario + "_Cupon3.Tif", NombreFormulario, NombreFormulario + "_Cupon3");
                                    //InsertRecorte("Huella", Directory + "Recorte\\Huella\\" + NombreFormulario + "_Huella3.BMP", NombreFormulario, NombreFormulario + "_Cupon3");

                                    InsertRecorte("Formulario", Directory + "Formulario\\" + NombreFormulario + ".TIF", NombreFormulario, NombreFormulario);

                                    #endregion

                                }
                                else
                                {
                                    int unio = 0;
                                }
                            }
                            else
                            {
                                FileInfo f = new FileInfo(imagen);
                                string Rut = f.DirectoryName.ToString() + "\\NoEncontradas\\";

                                DirectoryInfo Dir = new DirectoryInfo(Rut);
                                if (!Dir.Exists)
                                {
                                    Dir.Create();
                                }
                                try
                                {
                                    File.Move(imagen, Dir + f.Name);
                                }
                                catch (Exception ex)
                                {
                                    Log(ex.ToString());
                                }


                            }

                        }
                        else
                        {
                            bool EstaBien = false;
                            Dictionary<string, object> parametros = new Dictionary<string, object>();
                            parametros.Add("@Nombre", nombreDoc);
                            DataTable dt = ExecuteSPCon(conexion, "[dbo].[Traking_Transfer_Formulario]", parametros);
                            if (dt.Rows.Count > 0)
                            {


                                #region Variables
                                String NombreFormulario = dt.Rows[0][0].ToString();
                                String RutaFormulario = dt.Rows[0][1].ToString();
                                FileInfo Archivo = new FileInfo(imagen);
                                string HexaFormulario = Archivo.Name.ToUpper().Replace(".TIF", "").ToString();

                                FileInfo ArchivoCp1 = new FileInfo(Archivo.DirectoryName + "\\" + HexaFormulario + "_Cupon1.BMP");
                                if (!ArchivoCp1.Exists)
                                {
                                    string DosUltimos = HexaFormulario.Substring(HexaFormulario.Length - 2, 2);
                                    String NuevoDosUltimo = (int.Parse(DosUltimos) - 2).ToString().PadLeft(2, '0');
                                    String Comienzo = HexaFormulario.Substring(0, HexaFormulario.Length - 2);
                                    Comienzo = Comienzo + NuevoDosUltimo;
                                    FileInfo Archivo2Cp1 = new FileInfo(Archivo.DirectoryName + "\\" + Comienzo + "_Cupon1.BMP");
                                    if (Archivo2Cp1.Exists)
                                    {
                                        Archivo = new FileInfo(Archivo.DirectoryName + "\\" + Comienzo + ".TIF");
                                        EstaBien = true;
                                    }

                                }
                                else
                                {
                                    EstaBien = true;
                                }

                                if (EstaBien)
                                {


                                    string Directory = Archivo.Directory.ToString() + "\\";
                                    string Cupon1 = Archivo.Name.Replace(".TIF", "_Cupon1.BMP");
                                    string Cupon2 = Archivo.Name.Replace(".TIF", "_Cupon2.BMP");
                                    string Cupon3 = Archivo.Name.Replace(".TIF", "_Cupon3.BMP");
                                    string Cupon4 = Archivo.Name.Replace(".TIF", "_Cupon4.BMP");
                                    #endregion

                                    #region Renombra Archivo
                                    DirectoryInfo Dir = new DirectoryInfo(Directory + "Cupon\\");
                                    if (!Dir.Exists) { Dir.Create(); }

                                    DirectoryInfo Form = new DirectoryInfo(Directory + "Formulario\\");
                                    if (!Form.Exists) { Form.Create(); }

                                    string Directory2 = Directory + "Cupon\\";
                                    GuardarImagenTiff(300, Directory + Cupon1, RutaFormulario + NombreFormulario + "-Cupon1.Tif");
                                    GuardarImagenTiff(300, Directory + Cupon2, RutaFormulario + NombreFormulario + "-Cupon2.Tif");
                                    GuardarImagenTiff(300, Directory + Cupon3, RutaFormulario + NombreFormulario + "-Cupon3.Tif");
                                    GuardarImagenTiff(300, Directory + Cupon4, RutaFormulario + NombreFormulario + "-Cupon4.Tif");


                                    // GuardarImagenTiff(300, Directory + Cupon1, Directory2 + NombreFormulario + "-Cupon1.Tif");
                                    try { File.Copy(RutaFormulario + NombreFormulario + "-Cupon1.Tif", Directory2 + NombreFormulario + "-Cupon1.Tif", true); } catch (Exception ex) { Log(ex.ToString()); }
                                    try { File.Copy(RutaFormulario + NombreFormulario + "-Cupon2.Tif", Directory2 + NombreFormulario + "-Cupon2.Tif", true); } catch (Exception ex) { Log(ex.ToString()); }
                                    try { File.Copy(RutaFormulario + NombreFormulario + "-Cupon3.Tif", Directory2 + NombreFormulario + "-Cupon3.Tif", true); } catch (Exception ex) { Log(ex.ToString()); }
                                    try { File.Copy(RutaFormulario + NombreFormulario + "-Cupon4.Tif", Directory2 + NombreFormulario + "-Cupon4.Tif", true); } catch (Exception ex) { Log(ex.ToString()); }

                                    try { File.Move(Directory + Cupon1, Directory2 + NombreFormulario + "-Cupon1.BMP"); } catch (Exception ex) { Log(ex.ToString()); }
                                    try { File.Move(Directory + Cupon2, Directory2 + NombreFormulario + "-Cupon2.BMP"); } catch (Exception ex) { Log(ex.ToString()); }
                                    try { File.Move(Directory + Cupon3, Directory2 + NombreFormulario + "-Cupon3.BMP"); } catch (Exception ex) { Log(ex.ToString()); }

                                    try
                                    {
                                        File.Move(Directory + Cupon4, Directory2 + NombreFormulario + "-Cupon4.BMP");
                                    }
                                    catch
                                    {
                                        //File.Move(Directory + Cupon4.Replace("_Cupon", "_Cupo"), Directory2 + NombreFormulario + "-Cupon4.BMP");
                                    }

                                    try { File.Move(Directory + HexaFormulario + ".TIF", Directory + "Formulario\\" + NombreFormulario + ".TIF"); }
                                    catch (Exception ex) { Log(ex.ToString()); }

                                    #endregion

                                    #region Recorte Huella

                                    //Recorte(Directory2 + NombreFormulario + "-Cupon1.BMP", 500);
                                    //BinaryImage(Directory + "Recorte\\" + NombreFormulario + "-Recorte1.BMP");
                                    //// File.Delete(Directory + "Recorte\\" + NombreFormulario + "_Recorte1.BMP");

                                    //Recorte(Directory2 + NombreFormulario + "-Cupon2.BMP", 500);
                                    //BinaryImage(Directory + "Recorte\\" + NombreFormulario + "-Recorte2.BMP");
                                    //// File.Delete(Directory + "Recorte\\" + NombreFormulario + "_Recorte2.BMP");

                                    //Recorte(Directory2 + NombreFormulario + "-Cupon3.BMP", 500);
                                    //BinaryImage(Directory + "Recorte\\" + NombreFormulario + "-Recorte3.BMP");
                                    ////  File.Delete(Directory + "Recorte\\" + NombreFormulario + "_Recorte3.BMP");

                                    //try
                                    //{
                                    //	Recorte(Directory2 + NombreFormulario + "-Cupon4.BMP", 500);
                                    //	BinaryImage(Directory + "Recorte\\" + NombreFormulario + "-Recorte4.BMP");
                                    //}
                                    //catch
                                    //{
                                    //	Recorte(Directory2 + NombreFormulario + "-Cupo4.BMP", 500);
                                    //	BinaryImage(Directory + "Recorte\\" + NombreFormulario + "-Recorte4.BMP");
                                    //}
                                    //  File.Delete(Directory + "Recorte\\" + NombreFormulario + "_Recorte4.BMP");
                                    #endregion

                                    //File.Delete(Directory2 + NombreFormulario + "_Cupon1.BMP");
                                    //File.Delete(Directory2 + NombreFormulario + "_Cupon2.BMP");
                                    //File.Delete(Directory2 + NombreFormulario + "_Cupon3.BMP");
                                    //try
                                    //{
                                    //	File.Delete(Directory2 + NombreFormulario + "_Cupon4.BMP");
                                    //}
                                    //catch
                                    //{
                                    //	File.Delete(Directory2 + NombreFormulario + "_Cupo4.BMP");
                                    //}
                                    #region trazabilidad
                                    InsertRecorte("Cupon", Directory + "Cupon\\" + NombreFormulario + "_Cupon1.Fif", NombreFormulario, NombreFormulario + "_Cupon1");
                                    //InsertRecorte("Huella", Directory + "Recorte\\Huella\\" + NombreFormulario + "_Huella1.BMP", NombreFormulario, NombreFormulario + "_Cupon1");

                                    InsertRecorte("Cupon", Directory + "Cupon\\" + NombreFormulario + "_Cupon2.Tif", NombreFormulario, NombreFormulario + "_Cupon2");
                                    //InsertRecorte("Huella", Directory + "Recorte\\Huella\\" + NombreFormulario + "_Huella2.BMP", NombreFormulario, NombreFormulario + "_Cupon2");

                                    InsertRecorte("Cupon", Directory + "Cupon\\" + NombreFormulario + "_Cupon3.Tif", NombreFormulario, NombreFormulario + "_Cupon3");
                                    //InsertRecorte("Huella", Directory + "Recorte\\Huella\\" + NombreFormulario + "_Huella3.BMP", NombreFormulario, NombreFormulario + "_Cupon3");

                                    try
                                    {
                                        InsertRecorte("Cupon", Directory + "Cupon\\" + NombreFormulario + "_Cupon4.Tif", NombreFormulario, NombreFormulario + "_Cupon4");
                                        //InsertRecorte("Huella", Directory + "Recorte\\Huella\\" + NombreFormulario + "_Huella4.BMP", NombreFormulario, NombreFormulario + "_Cupon4");

                                    }
                                    catch
                                    {
                                        InsertRecorte("Cupon", Directory + "Cupon\\" + NombreFormulario + "_Cupo4.Tif", NombreFormulario, NombreFormulario + "_Cupon4");
                                        //InsertRecorte("Huella", Directory + "Recorte\\Huella\\" + NombreFormulario + "_Huella4.BMP", NombreFormulario, NombreFormulario + "_Cupon4");

                                    }
                                    InsertRecorte("Formulario", Directory + "Formulario\\" + NombreFormulario + ".TIF", NombreFormulario, NombreFormulario);

                                    #endregion

                                }
                                else
                                {
                                    int unio = 0;
                                }
                            }
                            else
                            {
                                FileInfo f = new FileInfo(imagen);
                                string Rut = f.DirectoryName.ToString() + "\\NoEncontradas\\";

                                DirectoryInfo Dir = new DirectoryInfo(Rut);
                                if (!Dir.Exists)
                                {
                                    Dir.Create();
                                }
                                try
                                {
                                    File.Move(imagen, Dir + f.Name);
                                }
                                catch (Exception ex)
                                {
                                    Log(ex.ToString());
                                }


                            }
                        }
                    }
                    else
                    {
                        InsertSalida(nombreDoc, imagen, usuario);
                    }
                }

                GC.Collect();
                return (int)ehReturnValue.EV_OK;
            }
            catch (Exception err)
            {
                Log(err.ToString());
                // MessageBox.Show(err.Message + " ## " + err.InnerException != null ? err.InnerException.Message : string.Empty + " ## " + err.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                GC.Collect();
                return (int)ehReturnValue.EV_OK;
            }
        }



        #endregion

        #region Metodos Propios

        private void GuardarImagenTiff(int dpi, string RutaOrigen, string RutaDestino)
        {
            try
            {
                byte[] fileData = null;

                using (FileStream fs = new FileStream(RutaOrigen,FileMode.Open, FileAccess.Read))
                {
                    var binaryReader = new BinaryReader(fs);
                    fileData = binaryReader.ReadBytes((int)fs.Length);
                    //  byte[] data = File.ReadAllBytes(fs.Length);

                    //ImageConverter IC = new ImageConverter();
                    //Image img = IC.ConvertFrom(data) as Image;

                    using (MemoryStream ms = new MemoryStream(fileData))
                {
                    using (Bitmap bitmap = (Bitmap)Image.FromStream(ms, true, true))
                    {
                        FileInfo f = new FileInfo(RutaDestino);
                        DirectoryInfo Dir = new DirectoryInfo(f.DirectoryName);
                        if (!Dir.Exists)
                            Dir.Create();


                        Image imgRecortada = Recortar.Recorta(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
                        using (Bitmap newBitmap = new Bitmap(imgRecortada))
                        {
                            // Bitmap bmp1 = new Bitmap(Dato);
                            ImageCodecInfo myImageCodecInfo;
                            myImageCodecInfo = GetEncoderInfo("image/tiff");
                            ImageCodecInfo jgpEncoder;

                            EncoderParameter myEncoderParameter;
                            jgpEncoder = GetEncoder(ImageFormat.Tiff);
                            System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Compression;
                            myEncoderParameter = new EncoderParameter(myEncoder, 4L);


                            EncoderParameters myEncoderParameters = new EncoderParameters(1);
                            myEncoderParameters.Param[0] = myEncoderParameter;
                            newBitmap.SetResolution(dpi, dpi);
                            FileInfo d = new FileInfo(RutaDestino);
                            if (d.Exists)
                            {
                                File.Move(RutaDestino, RutaDestino.ToUpper().ToString().Replace(".TIF", "_1.TIF"));
                            }


                            newBitmap.Save(RutaDestino, myImageCodecInfo, myEncoderParameters);


                            myEncoderParameter.Dispose();
                            myEncoderParameters.Dispose();
                     
                            newBitmap.Dispose();

                        }
                        imgRecortada.Dispose();
                        bitmap.Dispose();

                    }

                    ms.Dispose();
                   
                }
                    fs.Dispose();
                }

            }
            catch (Exception ex)
            {

                try
                {

                    FileInfo Dat = new FileInfo(RutaOrigen.Replace("Cupon", "Cupo"));
                    using (Bitmap bitmap = (Bitmap)Image.FromFile(RutaOrigen.Replace("Cupon", "Cupo")))
                    {
                        FileInfo Archivo = new FileInfo(RutaOrigen);
                        String Directorio = RutaOrigen;
                        FileInfo f = new FileInfo(RutaDestino);
                        DirectoryInfo Dir = new DirectoryInfo(f.DirectoryName);
                        if (!Dir.Exists)
                            Dir.Create();


                        Image imgRecortada = Recortar.Recorta(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
                        using (Bitmap newBitmap = new Bitmap(imgRecortada))
                        {
                            // Bitmap bmp1 = new Bitmap(Dato);
                            ImageCodecInfo myImageCodecInfo;
                            myImageCodecInfo = GetEncoderInfo("image/tiff");
                            ImageCodecInfo jgpEncoder;

                            EncoderParameter myEncoderParameter;


                            jgpEncoder = GetEncoder(ImageFormat.Tiff);
                            System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Compression;
                            myEncoderParameter = new EncoderParameter(myEncoder, 4L);


                            EncoderParameters myEncoderParameters = new EncoderParameters(1);
                            myEncoderParameters.Param[0] = myEncoderParameter;
                            newBitmap.SetResolution(dpi, dpi);
                            newBitmap.Save(RutaDestino, myImageCodecInfo, myEncoderParameters);
                            myEncoderParameter.Dispose();
                            myEncoderParameters.Dispose();
                            newBitmap.Dispose();

                        }
                        imgRecortada.Dispose();
                        bitmap.Dispose();
                    }

                }
                catch (Exception ex3)
                {
                    Log(ex3.ToString());
                    MessageBox.Show("Error Interno, El Formulario esta incompleto, Para Continuar Presione OK.");
                }

            }


            //Bitmap bmp1 = new Bitmap(RutaOrigen);
            //ImageCodecInfo jgpEncoder = GetEncoder(ImageFormat.Tiff);
            //System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Compression;
            //EncoderParameters myEncoderParameters = new EncoderParameters(1);
            //EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 4L);
            //bmp1.SetResolution(dpi, dpi);
            //myEncoderParameters.Param[0] = myEncoderParameter;
            //bmp1.Save(RutaDestino, jgpEncoder, myEncoderParameters);

        }

        private void InsertRecorte(string Tipo, string Ubicacion, string NombreFormulario, string NombreArchivo)
        {
            try
            {
                Dictionary<string, object> bu = new Dictionary<string, object>();
                bu.Add("@Tipo", Tipo);
                bu.Add("@Ubicacion", Ubicacion);
                bu.Add("@Formulario", NombreFormulario);
                bu.Add("@NombreArchivo", NombreArchivo);
                bu.Add("@opcion", "1");
                ExecuteSPCon(conexion, "dbo.Traking_Recorte_Sp", bu);
            }
            catch (Exception es)
            {
                Log(es.ToString());
            }

        }

        public void BinaryImage(String Ruta)
        {
            FileInfo Archivo = new FileInfo(Ruta);
            Bitmap source = new Bitmap(Ruta);
            Bitmap target = new Bitmap(source.Width, source.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            char[,] matrix = new char[source.Width, source.Height];

            matrix = CrearMatriz(Ruta, 100);

            int N_XX = NewGetDimensiones(4, source.Width, source.Height, matrix);
            int N_YY = NewGetDimensiones(3, source.Width, source.Height, matrix);
            int N_YY2 = NewGetDimensiones(5, source.Width, source.Height, matrix);

            int Diferencia = (N_YY2 - N_YY);
            Diferencia = 600 - Math.Abs(Diferencia);

            Diferencia = Math.Abs(Diferencia);
            if (Diferencia < 600)
            {
                Diferencia = Diferencia / 2;
                N_YY = N_YY - Diferencia;
                if (N_YY < 0)
                {
                    N_YY = 0;
                }
            }

            if (source.Width < (N_XX + 600))
            {
                N_XX = 200;
            }
            if (source.Height < (N_YY + 600))
            {
                N_YY = source.Height - 610;
            }
            string RutaHuella = Ruta;
            RecortarImagen(RutaHuella, RutaHuella, N_XX, N_YY, 600, 600, "Huellas\\NuevasPruebas\\prueba2\\" + Archivo.Name.Replace(".tif", ".bmp"), 500, 2);
            source.Dispose();
            target.Dispose();


        }

        private char[,] CrearMatriz(string Ruta, int Umbral)
        {
            try
            {
                Bitmap source = new Bitmap(Ruta);
                Bitmap target = new Bitmap(source.Width, source.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                char[,] matrix = new char[source.Width, source.Height];
                for (int i = 0; i < source.Width; i++)
                {
                    char[] b;
                    string Valor = "";
                    for (int e = 0; e < source.Height; e++)
                    {
                        // Color del pixel
                        Color col = source.GetPixel(i, e);
                        // Escala de grises
                        byte gray = (byte)(col.R * 0.3f + col.G * 0.59f + col.B * 0.11f);
                        if (col.B.ToString() != "255" && col.B.ToString() != "204" && col.B.ToString() != "192")
                        {
                            int uno = 1;
                        }
                        // Blanco o negro
                        byte value = 1;
                        char Va;
                        Va = '1';

                        if (gray > Umbral)
                        {
                            value = 0;
                            Va = '0';
                        }
                        else
                        {
                            Va = '1';
                        }
                        matrix[i, e] = Va;
                        Valor = Valor + value;
                        // Asginar nuevo color
                        Color newColor = System.Drawing.Color.FromArgb(value, value, value);
                        target.SetPixel(i, e, newColor);
                    }
                }
                source.Dispose();
                target.Dispose();
                GC.Collect();
                return matrix;

            }
            catch (Exception x)
            {
                Log(x.ToString());
                GC.Collect();
                return null;
            }
        }

        private int NewGetDimensiones(int opcion, int x_, int y_, char[,] matrix)
        {
            try
            {
                int Minx = 350;
                int MinY = 350;
                int MaxY = 1050;



                int MaxTOPY = y_ - 50;
                int resultado;



                if (opcion == 1)
                {//Y
                    resultado = 1000000;
                    Dictionary<int, int> bu = new Dictionary<int, int>();
                    int CountColumna = 0;

                    for (int x = 0; x < x_; x++)
                    {
                        for (int y = 0; y < y_; y++)
                        {
                            string valor = matrix[x, y].ToString();

                            if (valor == "1")
                            {
                                if (y != 0)
                                {
                                    GC.Collect();
                                    return y;
                                }

                            }

                        }
                    }
                    GC.Collect();
                    return resultado;
                }

                if (opcion == 2)
                {//Y
                    resultado = 1000000;
                    Dictionary<int, int> bu = new Dictionary<int, int>();
                    int CountColumna = 0;

                    for (int x = x_ - 1; x > 1; x--)
                    {

                        for (int y = y_ - 1; y > 1; y--)
                        {
                            string valor = matrix[x, y].ToString();
                            if (valor == "1")
                            {
                                GC.Collect();
                                return y;

                            }
                        }
                    }
                    GC.Collect();
                    return resultado;
                }


                if (opcion == 3)
                {//Y
                    resultado = 1000000;
                    Dictionary<int, int> bu = new Dictionary<int, int>();
                    int CountFila = 0;

                    for (int y = 0; y < y_; y++)
                    {
                        for (int x = 0; x < x_; x++)
                        {
                            if (x == 790)
                            {
                                int uno = 1;
                            }
                            string valor = matrix[x, y].ToString();
                            if (valor == "1")
                            {

                                if (MinY <= y)
                                {
                                    CountFila++;
                                    if (resultado > y)
                                    {
                                        resultado = y;
                                    }
                                }
                                else
                                {
                                    int uno = 1;
                                }

                            }

                        }
                        if (MinY >= y)
                        {
                            bu.Add(y, 0);
                        }
                        else
                        {
                            bu.Add(y, CountFila);
                        }

                        CountFila = 0;

                    }

                    int res = GetPosition(bu, 1);
                    GC.Collect();
                    return res;
                }




                if (opcion == 4)
                {//X
                    int CountColumna = 0;
                    resultado = 10000;
                    Dictionary<int, int> bu = new Dictionary<int, int>();
                    int X_Anterior = 0;

                    for (int x = 0; x < x_; x++)
                    {
                        for (int y = 0; y < y_; y++)
                        {
                            string valor = matrix[x, y].ToString();
                            if (valor == "1")
                            {
                                if (Minx <= x)
                                {
                                    CountColumna++;
                                    if (resultado > x)
                                    {
                                        resultado = x;
                                    }
                                }
                            }
                        }

                        if (Minx >= x)
                        {
                            bu.Add(x, 0);
                        }
                        else
                        {
                            bu.Add(x, CountColumna);
                        }
                        CountColumna = 0;
                    }
                    int res = GetPosition(bu, 1);
                    GC.Collect();
                    return res;
                }


                if (opcion == 5)
                {//Y
                    resultado = 1000000;
                    Dictionary<int, int> bu = new Dictionary<int, int>();
                    int CountFila = 0;

                    for (int y = y_ - 1; y > 1; y--)
                    {

                        for (int x = x_ - 1; x > 1; x--)
                        {

                            string valor = matrix[x, y].ToString();
                            if (valor == "1")
                            {

                                if (MaxY >= y)
                                {
                                    CountFila++;
                                    if (resultado > y)
                                    {
                                        resultado = y;
                                    }
                                }
                                else
                                {
                                    int uno = 1;
                                }

                            }

                        }
                        bu.Add(y, CountFila);
                        CountFila = 0;

                    }
                    int res = GetPosition(bu, 1);
                    GC.Collect();
                    return res;
                }



                GC.Collect();
                return 636;
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
                GC.Collect();
                return 0;

            }
        }

        private int GetPosition(Dictionary<int, int> bu, int Opcion)
        {
            try
            {
                int Umbrahuella = 35;
                int Resultado = 350;

                if (Opcion == 1)
                {
                    foreach (KeyValuePair<int, int> Par in bu)
                    {
                        if (Par.Value >= Umbrahuella)
                        {
                            GC.Collect();
                            return Par.Key;
                        }
                    }

                }
                GC.Collect();
                return Resultado;
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
                GC.Collect();
                return 0;
            }
        }

        private int GetDimensiones(int opcion, int x_, int y_, char[,] matrix)
        {
            try
            {
                int Minx = 150;
                int MinY = 150;
                int resultado;
                if (opcion == 1)
                {//N_Alto
                    resultado = 0;


                    for (int y = y_ - 1; y > 1; y--)
                    {
                        for (int x = x_ - 1; x > 1; x--)
                        {
                            string valor = matrix[x, y].ToString();
                            if (valor == "1")
                            {
                                if (resultado < y)
                                {
                                    resultado = y;
                                }

                            }
                        }

                    }
                    GC.Collect();
                    return resultado;
                }

                if (opcion == 2)
                {//N_Ancho
                    resultado = 0;

                    for (int y = y_ - 1; y > 1; y--)
                    {
                        for (int x = x_ - 1; x > 1; x--)
                        {
                            string valor = matrix[x, y].ToString();
                            if (valor == "1")
                            {
                                if (resultado < x)
                                {
                                    resultado = x;
                                }

                            }
                        }
                    }
                    GC.Collect();
                    return resultado;
                }
                if (opcion == 3)
                {//Y
                    resultado = 1000000;


                    for (int y = y_ - 1; y > 1; y--)
                    {
                        for (int x = x_ - 1; x > 1; x--)
                        {
                            string valor = matrix[x, y].ToString();
                            if (valor == "1")
                            {
                                if (MinY <= y)
                                {

                                    if (resultado > y)
                                    {
                                        resultado = y;
                                    }
                                }
                                else
                                {
                                    int uno = 1;
                                }

                            }

                        }

                    }
                    GC.Collect();
                    return resultado;
                }
                if (opcion == 4)
                {//X
                    resultado = 10000;


                    for (int y = y_ - 1; y > 1; y--)
                    {
                        for (int x = x_ - 1; x > 1; x--)
                        {
                            string valor = matrix[x, y].ToString();
                            if (valor == "1")
                            {
                                if (Minx <= x)
                                {

                                    if (resultado > x)
                                    {
                                        resultado = x;
                                    }
                                }
                            }
                        }

                    }
                    GC.Collect();
                    return resultado;
                }




                GC.Collect();
                return 0;
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
                GC.Collect();
                return 0;

            }
        }

        private void RecortarImagen(string Dato, string SepararImagenRutaRecorte, int x, int y, int Alto, int Ancho, string nombre, int dpi, int Opcion)
        {
            try
            {
                string Tipo = "Cupon";
                FileInfo Dat = new FileInfo(Dato);
                string Pagina = Dato.Split('\\')[Dato.Split('\\').Length - 3].ToString();
                nombre = "\\Huella\\" + Dat.Name.ToString();


                //string rentrada = RutaIniciar; string rsalida = RutaDestinoP;

                using (Bitmap bitmap = (Bitmap)Image.FromFile(Dato))
                {
                    FileInfo Archivo = new FileInfo(Dato);
                    String RutaFinal = SepararImagenRutaRecorte;
                    FileInfo f = new FileInfo(RutaFinal);
                    DirectoryInfo Dir = new DirectoryInfo(f.DirectoryName + "\\Huella\\");
                    if (!Dir.Exists)
                    {
                        Dir.Create();
                    }

                    Image imgRecortada = Recortar.Recorta(bitmap, new Rectangle(x, y, Ancho, Alto));
                    using (Bitmap newBitmap = new Bitmap(imgRecortada))
                    {
                        // Bitmap bmp1 = new Bitmap(Dato);
                        ImageCodecInfo myImageCodecInfo;
                        myImageCodecInfo = GetEncoderInfo("image/tiff");
                        ImageCodecInfo jgpEncoder;

                        EncoderParameter myEncoderParameter;
                        if (Opcion == 2)
                        {

                            jgpEncoder = GetEncoder(ImageFormat.Bmp);
                            System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.ColorDepth;
                            myEncoderParameter = new EncoderParameter(myEncoder, 8L);
                        }
                        else if (Opcion == 1)
                        {
                            jgpEncoder = GetEncoder(ImageFormat.Tiff);
                            //System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Compression;
                            System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Compression;
                            myEncoderParameter = new EncoderParameter(myEncoder, 4L);
                        }
                        else
                        {
                            jgpEncoder = GetEncoder(ImageFormat.Tiff);
                            System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.ColorDepth;
                            myEncoderParameter = new EncoderParameter(myEncoder, 8L);
                        }

                        EncoderParameters myEncoderParameters = new EncoderParameters(1);
                        myEncoderParameters.Param[0] = myEncoderParameter;
                        newBitmap.SetResolution(dpi, dpi);
                        // newBitmap.Save(Dir.ToString() + Archivo.Name.Replace("-Recorte", "_Huella"), myImageCodecInfo, myEncoderParameters);


                        //newBitmap.Save(RutaFinal, ImageFormat.Tiff);


                        myEncoderParameter.Dispose();
                        myEncoderParameters.Dispose();
                        newBitmap.Dispose();

                    }

                    if (Opcion == 2 || Opcion == 3)
                    {
                        GirarImgane(RutaFinal);
                        Tipo = "Huella";
                    }


                    #region trazabilidad
                    //FileInfo NArchivo = new FileInfo(RutaFinal);
                    //FileInfo NFormulario = new FileInfo(Dato);
                    //Dictionary<string, object> bu = new Dictionary<string, object>();
                    //bu.Add("@Tipo", Tipo);
                    //bu.Add("@Ubicacion", RutaFinal);
                    //bu.Add("@Formulario", NFormulario.Name.Replace(NFormulario.Extension, ""));
                    //bu.Add("@NombreArchivo", NArchivo.Name.Replace(NArchivo.Extension, ""));
                    //bu.Add("@opcion", "1");
                    //ExecuteSPCon(conexion,"dbo.Traking_Recorte_Sp", bu);


                    #endregion

                    bitmap.Dispose();



                }

            }
            catch (Exception ex)
            {
                Log(ex.ToString());
                MessageBox.Show("Error Interno, El Formulario esta incompleto, Para Continuar Presione ACEPTAR..");
            }
        }

        private void Recorte(string Dato, int dpi)
        {
            try
            {

                FileInfo Dat = new FileInfo(Dato);
                string Pagina = Dato.Split('\\')[Dato.Split('\\').Length - 3].ToString();
                using (Bitmap bitmap = (Bitmap)Image.FromFile(Dato))
                {
                    FileInfo Archivo = new FileInfo(Dato);
                    String Directorio = Dato;
                    String RutaFinal = Dato.Replace("Cupon", "Recorte");
                    FileInfo f = new FileInfo(RutaFinal);
                    DirectoryInfo Dir = new DirectoryInfo(f.DirectoryName);
                    if (!Dir.Exists)
                        Dir.Create();

                    int x = bitmap.Width - 1100;
                    int y = 0;
                    int Alto = bitmap.Height;
                    int Ancho = 1090;

                    Image imgRecortada = Recortar.Recorta(bitmap, new Rectangle(x, y, Ancho, Alto));
                    using (Bitmap newBitmap = new Bitmap(imgRecortada))
                    {
                        // Bitmap bmp1 = new Bitmap(Dato);
                        ImageCodecInfo myImageCodecInfo;
                        myImageCodecInfo = GetEncoderInfo("image/tiff");
                        ImageCodecInfo jgpEncoder;

                        EncoderParameter myEncoderParameter;


                        jgpEncoder = GetEncoder(ImageFormat.Bmp);
                        System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.ColorDepth;
                        myEncoderParameter = new EncoderParameter(myEncoder, 8L);


                        EncoderParameters myEncoderParameters = new EncoderParameters(1);
                        myEncoderParameters.Param[0] = myEncoderParameter;
                        newBitmap.SetResolution(dpi, dpi);
                        newBitmap.Save(RutaFinal, myImageCodecInfo, myEncoderParameters);
                        myEncoderParameter.Dispose();
                        myEncoderParameters.Dispose();
                        newBitmap.Dispose();

                    }
                    bitmap.Dispose();
                }

            }
            catch (Exception ex)
            {
                Log(ex.ToString());
                MessageBox.Show("Error Interno, El Formulario esta incompleto, Para Continuar Presione ACEPTAR...");
            }
        }

        private bool ValidacaionDeCampos(string campo, string Buffer)
        {
            try
            {
                bool Resultado = false;
                long num5;


                switch (campo)
                {

                    case "cedula":
                        if (Buffer == "")
                        {
                            GC.Collect();
                            return false;
                        }
                        else if (long.TryParse(Buffer, out num5))
                        {
                            if (Buffer.Length >= 3 && Buffer.Length <= 10)
                            {
                                GC.Collect();
                                return true;
                            }
                        }
                        break;

                    case "Nom1":
                        if (Buffer == "")
                        {
                            GC.Collect();
                            return false;
                        }
                        else if (!long.TryParse(Buffer, out num5))
                        {
                            if (Buffer.Length >= 1 && Buffer.Length <= 60)
                            {
                                if (ValidarCaracteresEspeciales(Buffer))
                                {
                                    GC.Collect();
                                    return true;
                                }
                                else
                                {
                                    GC.Collect();
                                    return false;
                                }

                            }
                        }
                        break;
                    case "Nom2":
                        if (!long.TryParse(Buffer, out num5))
                        {
                            if (Buffer == "")
                            {
                                GC.Collect();
                                return true;
                            }
                            else if (Buffer.Length <= 60)
                            {
                                if (ValidarCaracteresEspeciales(Buffer))
                                {
                                    GC.Collect();
                                    return true;
                                }
                                else
                                {
                                    GC.Collect();
                                    return false;
                                }
                            }
                        }
                        break;
                    case "Ape1":
                        if (Buffer == "")
                        {
                            GC.Collect();
                            return false;
                        }
                        else if (!long.TryParse(Buffer, out num5))
                        {
                            if (Buffer.Length >= 2 && Buffer.Length <= 60)
                            {
                                if (ValidarCaracteresEspeciales(Buffer))
                                {
                                    GC.Collect();
                                    return true;
                                }
                                else
                                {
                                    GC.Collect();
                                    return false;
                                }
                            }
                        }
                        break;
                    case "Ape2":
                        if (!long.TryParse(Buffer, out num5))
                        {
                            if (Buffer == "")
                            {
                                GC.Collect();
                                return true;
                            }
                            else
                            if (Buffer.Length <= 60)
                            {
                                if (ValidarCaracteresEspeciales(Buffer))
                                {
                                    GC.Collect();
                                    return true;
                                }
                                else
                                {
                                    GC.Collect();
                                    return false;
                                }
                            }
                        }
                        break;
                    case "fexpedia":

                        if (Buffer == "")
                        {
                            GC.Collect();
                            return false;
                        }
                        else if (long.TryParse(Buffer, out num5))
                        {
                            if (Buffer.Length == 2)
                            {
                                if (int.Parse(Buffer) != 0 && int.Parse(Buffer) <= 31)
                                {
                                    GC.Collect();
                                    return true;
                                }
                            }
                        }
                        break;
                    case "fexpemes":

                        if (Buffer == "")
                        {
                            GC.Collect();
                            return false;
                        }
                        else if (long.TryParse(Buffer, out num5))
                        {
                            if (Buffer.Length == 2)
                            {
                                if (int.Parse(Buffer) != 0 && int.Parse(Buffer) <= 12)
                                {
                                    GC.Collect();
                                    return true;
                                }
                            }
                        }
                        break;
                    case "fexpeano":

                        if (Buffer == "")
                        {
                            GC.Collect();
                            return false;
                        }
                        else if (long.TryParse(Buffer, out num5))
                        {
                            if (Buffer.Length == 4)
                            {
                                if (1900 <= int.Parse(Buffer) && int.Parse(Buffer) <= 2017)
                                {
                                    GC.Collect();
                                    return true;
                                }
                            }
                        }
                        break;

                    case "finsdia":

                        if (Buffer == "")
                        {
                            GC.Collect();
                            return false;
                        }
                        else if (long.TryParse(Buffer, out num5))
                        {
                            if (Buffer.Length == 2)
                            {
                                if (int.Parse(Buffer) != 0 && int.Parse(Buffer) <= 31)
                                {
                                    GC.Collect();
                                    return true;
                                }
                            }
                        }
                        break;
                    case "finsmes":

                        if (Buffer == "")
                        {
                            GC.Collect();
                            return false;
                        }
                        else if (long.TryParse(Buffer, out num5))
                        {
                            if (Buffer.Length == 2)
                            {
                                if (int.Parse(Buffer) != 0 && int.Parse(Buffer) <= 12)
                                {
                                    GC.Collect();
                                    return true;
                                }
                            }
                        }
                        break;
                    case "finsano":

                        if (Buffer == "")
                        {
                            GC.Collect();
                            return false;
                        }
                        else if (long.TryParse(Buffer, out num5))
                        {
                            if (Buffer.Length == 1)
                            {
                                if (int.Parse(Buffer) >= 0 && int.Parse(Buffer) <= 9)
                                {
                                    GC.Collect();
                                    return true;
                                }
                            }
                        }
                        break;

                    case "Nvlestudio":
                        if (Buffer == "")
                        {
                            GC.Collect();
                            return true;
                        }
                        else if (long.TryParse(Buffer, out num5))
                        {
                            if (int.Parse(Buffer) <= 4)
                            {
                                GC.Collect();
                                return true;
                            }
                        }
                        break;
                    case "Invidente":
                        if (Buffer == "")
                        {
                            GC.Collect();
                            return true;
                        }
                        if (long.TryParse(Buffer, out num5))
                        {
                            if (int.Parse(Buffer) == 0 || int.Parse(Buffer) == 1)
                            {
                                GC.Collect();
                                return true;
                            }
                        }
                        break;
                    case "Braile":
                        if (Buffer == "")
                        {
                            GC.Collect();
                            return true;
                        }
                        if (long.TryParse(Buffer, out num5))
                        {
                            if (int.Parse(Buffer) == 0 || int.Parse(Buffer) == 1)
                            {
                                GC.Collect();
                                return true;
                            }
                        }
                        break;
                    case "Genero":
                        if (Buffer == "")
                        {
                            GC.Collect();
                            return true;
                        }
                        if (long.TryParse(Buffer, out num5))
                        {
                            if (int.Parse(Buffer) == 1 || int.Parse(Buffer) == 2)
                            {
                                GC.Collect();
                                return true;
                            }
                        }
                        break;
                    case "genero":
                        if (Buffer == "")
                        {
                            GC.Collect();
                            return true;
                        }
                        if (long.TryParse(Buffer, out num5))
                        {
                            if (int.Parse(Buffer) == 1 || int.Parse(Buffer) == 2)
                            {
                                GC.Collect();
                                return true;
                            }
                        }
                        break;
                    case "Capacidad":
                        if (Buffer == "")
                        {
                            GC.Collect();
                            return true;
                        }
                        if (long.TryParse(Buffer, out num5))
                        {
                            if (int.Parse(Buffer) == 1 || int.Parse(Buffer) == 2)
                            {
                                GC.Collect();
                                return true;
                            }
                        }
                        break;

                    case "Dir":
                        GC.Collect();
                        return true;
                        break;
                    case "TelFijo":
                        if (Buffer == "")
                        {
                            GC.Collect();
                            return true;
                        }
                        if (ValidarTelefono(Buffer))
                        {
                            string imagen = EHFApp.Form.GetSourceImage();
                            String Formulario = imagen.Split('\\')[imagen.Split('\\').Length - 1];
                            Formulario = Formulario.Substring(0, Formulario.Length - 11);
                            Dictionary<string, object> parametros = new Dictionary<string, object>();
                            parametros.Add("@Nombre", Formulario);
                            DataTable dt = ExecuteSPCon(conexion, "[dbo].[Traking_Get_TipoInscripcion]", parametros);

                            if (dt.Rows.Count > 0)
                            {
                                string tipoInscripcion = dt.Rows[0][2].ToString();
                                if (tipoInscripcion == "2")
                                {
                                    /* if (Buffer.Length <= 7)*/
                                    if (Buffer.Length <= 20)
                                    {
                                        GC.Collect();
                                        return true;
                                    }
                                }
                                else if (tipoInscripcion == "3")
                                {
                                    /*if (Buffer.Length <= 10)*/
                                    if (Buffer.Length <= 20)
                                    {
                                        GC.Collect();
                                        return true;
                                    }
                                }

                            }
                        }
                        if (long.TryParse(Buffer, out num5))
                        {

                        }
                        break;
                    case "TelMovil":
                        if (Buffer == "")
                        {
                            GC.Collect();
                            return true;
                        }
                        if (ValidarTelefono(Buffer))
                        {
                            string imagen = EHFApp.Form.GetSourceImage();
                            String Formulario = imagen.Split('\\')[imagen.Split('\\').Length - 1];
                            Formulario = Formulario.Substring(0, Formulario.Length - 11);
                            Dictionary<string, object> parametros = new Dictionary<string, object>();
                            parametros.Add("@Nombre", Formulario);
                            DataTable dt = ExecuteSPCon(conexion, "[dbo].[Traking_Get_TipoInscripcion]", parametros);

                            if (dt.Rows.Count > 0)
                            {
                                string tipoInscripcion = dt.Rows[0][2].ToString();
                                if (tipoInscripcion == "2")
                                {
                                    /*if (Buffer.Length <= 10)*/
                                    if (Buffer.Length <= 20)
                                    {
                                        GC.Collect();
                                        return true;
                                    }
                                }
                                else if (tipoInscripcion == "3")
                                {
                                    /* if (Buffer.Length <= 16)*/
                                    if (Buffer.Length <= 20)
                                    {
                                        GC.Collect();
                                        return true;
                                    }
                                }

                            }
                        }
                        break;
                    case "Email":

                        if (Buffer == "")
                        {
                            GC.Collect();
                            return true;
                        }
                        else
                        {
                            /* String expresion;
                             expresion = @"(@)$";
                             if (Regex.IsMatch(Buffer.ToUpper(), expresion))
                             {
                                 if (Regex.Replace(Buffer.ToUpper(), expresion, String.Empty).Length == 0)
                                 {
                                     GC.Collect();
                                     return true;
                                 }
                             }
                             else
                             {
                                 GC.Collect();
                                 return false;
                             }*/
                            if (Buffer.Length <= 150)
                            {
                                GC.Collect();
                                return true;
                            }
                            else
                            {
                                GC.Collect();
                                return false;
                            }
                        }
                        break;
                    case "Anulado":
                        if (Buffer == "")
                        {
                            GC.Collect();
                            return false;
                        }
                        if (!long.TryParse(Buffer, out num5))
                        {
                            if (Buffer.Length == 2)
                            {
                                if (Buffer.ToUpper() == "SI" || Buffer.ToUpper() == "NO")
                                {
                                    GC.Collect();
                                    return true;
                                }

                            }
                        }
                        break;
                    case "enmenda":
                        if (Buffer == "")
                        {
                            GC.Collect();
                            return false;
                        }
                        if (!long.TryParse(Buffer, out num5))
                        {
                            if (Buffer.Length == 2)
                            {
                                if (Buffer.ToUpper() == "SI" || Buffer.ToUpper() == "NO")
                                {
                                    GC.Collect();
                                    return true;
                                }

                            }
                        }
                        break;
                    case "Huellas":
                        if (Buffer == "")
                        {
                            GC.Collect();
                            return false;
                        }
                        if (!long.TryParse(Buffer, out num5))
                        {
                            if (Buffer.Length == 2)
                            {
                                if (Buffer.ToUpper() == "SI" || Buffer.ToUpper() == "NO")
                                {
                                    GC.Collect();
                                    return true;
                                }

                            }
                        }
                        break;
                    case "Difhuella":
                        if (Buffer == "")
                        {
                            GC.Collect();
                            return false;
                        }
                        if (!long.TryParse(Buffer, out num5))
                        {
                            if (Buffer.Length == 2)
                            {
                                if (Buffer.ToUpper() == "SI" || Buffer.ToUpper() == "NO")
                                {
                                    GC.Collect();
                                    return true;
                                }

                            }
                        }
                        break;


                }
                GC.Collect();
                return false;
            }
            catch (Exception x)
            {
                Log(x.ToString());
                GC.Collect();
                return false;
            }
        }

        private bool ValidarTelefono(string buffer)
        {
            try
            {
                bool Resultado = false;

                string[] partNumbers = { buffer };
                Regex rgx = new Regex(@"^[1-9]$");
                foreach (string partNumber in partNumbers)
                {
                    string str = partNumber;
                    char[] arr;

                    arr = str.ToCharArray(0, partNumber.Length);
                    foreach (char d in arr)
                    {

                        if (char.IsNumber(d))
                        {
                            Resultado = true;
                        }
                        else
                        {
                            Resultado = false;
                            break;
                        }
                    }
                }

                return Resultado;
            }
            catch (Exception ex)
            { return false; }
        }

        private bool ValidarCaracteresEspeciales(string Buffer)
        {
            try
            {
                string[] partNumbers = { Buffer };
                Regex rgx = new Regex(@"^[a-zA-ZÑñ]$");
                bool Resultado = false;
                foreach (string partNumber in partNumbers)
                {
                    string str = partNumber;
                    char[] arr;

                    arr = str.ToCharArray(0, partNumber.Length);
                    foreach (char d in arr)
                    {
                        string valor = d.ToString();
                        if (rgx.IsMatch(valor))
                        {
                            Resultado = true;
                        }
                        else
                        {
                            Resultado = false;
                            break;
                        }
                    }
                }


                GC.Collect();
                return Resultado;
            }
            catch (Exception m)
            {
                Log(m.ToString());
                GC.Collect();
                return false;
            }
        }

        private bool validarSegundaCaptura(DataTable dt, string valor)
        {
            try
            {
                foreach (DataRow Datos in dt.Rows)
                {
                    if (Datos.ItemArray[0].ToString().ToUpper().TrimEnd().TrimStart() == valor.ToString().ToUpper().TrimEnd().TrimStart())
                    {
                        GC.Collect();
                        return true;
                    }
                }
                GC.Collect();
                return false;
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
                GC.Collect();
                return false;
            }
        }

        private void DeshabilitarMenues()
        {
            try
            {
                short[] array = new short[]
    {
    167,
    168,
    122,
    123,
    124,
    125,
    133,
    163,
    136,
    135
    };
                for (int i = 0; i < 10; i++)
                {
                    ((VerifyApp)this.EHFApp).VeriSetMenuItemEnabled(array[i], false);
                }
                GC.Collect();
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
                GC.Collect();
            }
        }

        private void GirarImgane(string RutaImagen)
        {
            try
            {
                Image bmp = Image.FromFile(RutaImagen);


                bmp.RotateFlip(RotateFlipType.Rotate90FlipXY);

                bmp.RotateFlip(RotateFlipType.Rotate270FlipXY);

                bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);

                bmp.Save(RutaImagen);

                bmp.Dispose();
                GC.Collect();

            }
            catch (Exception ex)
            {
                Log(ex.ToString());
                GC.Collect();
            }
        }

        private void InsertTrazabilidad(int proceso, bool completar = false)
        {
            try
            {
#region Variables
                DataTable ObjTrazabilidad = GetDataTableBasico();
                string Nombre = EHFApp.Form.GetEHKey();
                string usuario = EHFApp.GetUserName().Trim();
                string imagen = EHFApp.Form.GetSourceImage();


                if (EHFApp is InterpretApp)
                {
                    if (!EHFApp.Job.GetName().ToUpper().Contains("PRIMERA") && !EHFApp.Job.GetName().ToUpper().Contains("SEGUNDA"))
                    {


                    }
                    else
                    {
                        string NombreImagen = imagen.Split('\\')[imagen.Split('\\').Length - 1];
                        String Formulario = NombreImagen.Split('-')[0];
                        string Cupon = NombreImagen.Split('-')[1].Replace("Cupon", "").Replace(".Tif", "");
                        VerificacionInterpretacionCamposRequeridos(Formulario, Cupon);

                        EHFApp.Form.GetFormField("Anulado", 0).SetValueStr(string.Empty);
                        EHFApp.Form.GetFormField("Anulado", 0).SetStatus(ehFieldStatus.ehValidationError);
                        //EHFApp.Form.GetFormField("Email", 0).SetStatus(ehFieldStatus.ehValidationError);
                        EHFApp.Form.GetFormField("TelFijo", 0).SetStatus(ehFieldStatus.ehValidationError);
                        EHFApp.Form.GetFormField("Difhuella", 0).SetStatus(ehFieldStatus.ehValidationError);
                    }
                }
                short j = 2;
                ClassData.Braile = false;
                ClassData.Invidente = false;
                ClassData.Nvlestudio = false;
                ClassData.Genero = false;
                ClassData.Capacidad = false;
                string Cedula = "";
#endregion
                string an = "";

                if (EHFApp is InterpretApp)
                {
                    if (!EHFApp.Job.GetName().ToUpper().Contains("PRIMERA") && !EHFApp.Job.GetName().ToUpper().Contains("SEGUNDA"))
                    {
                        for (short i = 1; i <= EHFApp.Form.GetMaxNoOfFormFields(); i++)
                        {
                            string campo = EHFApp.Form.GetFormFieldNo(i).GetName().Trim();
                            string valor = EHFApp.Form.GetFormFieldNo(i).GetValueStr().ToUpper().Trim();
                            string estado = EHFApp.Form.GetFormFieldNo(i).GetStatus() == ehFieldStatus.ehComplete ? "1" : "0";
                            EHFApp.Form.GetFormFieldNo(i).SetValueStr("");
                            EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                        }

                        FileInfo Archivo = new FileInfo(imagen);
                        ObjTrazabilidad.Rows.Add("Formulario", Archivo.Name.Replace(Archivo.Extension, ""), "1", usuario);
                    }
                    else
                    {
#region Interpretacion
                        for (short i = 1; i <= EHFApp.Form.GetMaxNoOfFormFields(); i++)
                        {
                            string campo = EHFApp.Form.GetFormFieldNo(i).GetName().Trim();
                            string valor = EHFApp.Form.GetFormFieldNo(i).GetValueStr().ToUpper().Trim();
                            string estado = EHFApp.Form.GetFormFieldNo(i).GetStatus() == ehFieldStatus.ehComplete ? "1" : "0";

#region Data
                            if (campo == "Nvlestudio" || campo == "Invidente" || campo == "Braile" || campo == "Genero" || campo == "Capacidad")
                            {
                                //EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);

                                valor = ValidarCampos(campo, valor, ObjTrazabilidad);


                            }
                            if (campo == "cedula")
                            {
                                EHFApp.Form.GetFormFieldNo(i).SetValueStr("");
                            }
                            if (campo == "Anulado")
                            {
                                EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehValidationError);
                            }
                            if (campo == "enmenda")
                            {
                                EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehValidationError);
                            }
                            if (campo == "Difhuella")
                            {
                                EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehValidationError);
                            }
                            if (campo.StartsWith("f"))
                            {
                                EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehValidationError);
                            }


#endregion
                            if (valor != "%")
                            {
                                if (valor.Contains("*"))
                                {
                                    valor = "";
                                    EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehRetype);
                                    EHFApp.Form.GetFormFieldNo(i).SetValueStr(valor);
                                }
                                ObjTrazabilidad.Rows.Add(campo, valor, estado, usuario);
                            }
                            else if (valor == "%")
                            {
                                EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                            }


                        }
#endregion
                    }
                }
                else
                {
#region CATURPA
                    if (EHFApp.Job.GetName().ToUpper().Contains("PRIMERA"))
                    {
                        an = EHFApp.Form.GetFormField("Anulado", 0).GetValueStr();
                        if (an == "SI")
                        {
                            ClassData.Braile = false;
                            ClassData.Invidente = false;
                            ClassData.Nvlestudio = false;
                            ClassData.Genero = false;
                            ClassData.Capacidad = false;

                            //EHFApp.Form.GetFormField("Capacidad", 1).SetValueStr("");
                            //EHFApp.Form.GetFormField("Capacidad", 1).SetStatus(ehFieldStatus.ehRetype);
                            //EHFApp.Form.GetFormField("Capacidad", 2).SetValueStr("");
                            //EHFApp.Form.GetFormField("Capacidad", 2).SetStatus(ehFieldStatus.ehComplete);


                            for (short i = 1; i <= EHFApp.Form.GetMaxNoOfFormFields(); i++)
                            {

                                string campo = EHFApp.Form.GetFormFieldNo(i).GetName().Trim();
                                string valor = EHFApp.Form.GetFormFieldNo(i).GetValueStr().ToUpper().Trim();
                                string estado = EHFApp.Form.GetFormFieldNo(i).GetStatus() == ehFieldStatus.ehComplete ? "1" : "0";

                                valor = "";
                                //EHFApp.Form.GetFormFieldNo(i).SetValueStr(string.Empty);
                                // EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                                if (campo != "Anulado")
                                {

                                    if (campo == "Braile" || campo == "Nvlestudio" || campo == "Invidente" || campo == "Genero" || campo == "Capacidad")
                                    {

                                        EHFApp.Form.GetFormFieldNo(i).SetValueStr(string.Empty);
                                        // EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                                        valor = ValidarCampos(campo, valor, ObjTrazabilidad);
                                        if (valor != "%")
                                        {
                                            ObjTrazabilidad.Rows.Add(campo, "", "0", usuario);
                                        }
                                        //else
                                        //{
                                        //    EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                                        //}
                                    }
                                    else
                                    {
                                        //if (campo =="finsdia")
                                        //{
                                        //    EHFApp.Form.GetFormField("finsdia", 0).SetValueStr("");
                                        //    EHFApp.Form.GetFormField("finsdia", 0).SetStatus(ehFieldStatus.ehComplete);
                                        //}
                                        // EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                                        // EHFApp.Form.SetStatus(ehFormStatus.ehInterpretError);
                                        ObjTrazabilidad.Rows.Add(campo, valor, estado, usuario);
                                    }
                                }
                                else
                                {
                                    // EHFApp.Form.GetFormField("Anulado", 0).SetValueStr(string.Empty);
                                    EHFApp.Form.GetFormField("Anulado", 0).SetStatus(ehFieldStatus.ehRetype);
                                    ObjTrazabilidad.Rows.Add(campo, "SI", "1", usuario);
                                }

                                //  EHFApp.Form.GetFormFieldNo(i).SetValueStr(string.Empty);

                            }

                            ObjTrazabilidad.AcceptChanges();
                            ExcuteTrazabilidad(Nombre, ObjTrazabilidad, 2);
                            EHFApp.Form.SetStatus(ehFormStatus.ehRetype);
                            //EHFApp.Form.SetQueues(2);
                            //(EHFApp as VerifyApp).SkipForm(1);
                        }
                        else
                        {
                            for (short i = 1; i <= EHFApp.Form.GetMaxNoOfFormFields(); i++)
                            {
                                string campo = EHFApp.Form.GetFormFieldNo(i).GetName().Trim();
                                string valor = EHFApp.Form.GetFormFieldNo(i).GetValueStr().ToUpper().Trim();
                                string estado = EHFApp.Form.GetFormFieldNo(i).GetStatus() == ehFieldStatus.ehComplete ? "1" : "0";

                                if (campo == "cedula")
                                {
                                    //  ValidarDataRefence(valor);
                                    Cedula = valor;
                                    // EHFApp.Form.GetFormFieldNo(i).SetValueStr("");
                                }
                                else if (campo == "Nvlestudio" || campo == "Genero" || campo == "Capacidad")
                                {
                                    EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                                    valor = ValidarCampos(campo, valor, ObjTrazabilidad);
                                }
                                else
                                {
                                    // EHFApp.Form.GetFormFieldNo(i).SetValueStr("");
                                }


                                if (valor != "%")
                                {
                                    if (EHFApp.Form.GetName().StartsWith("Huella"))
                                    {
                                        ObjTrazabilidad.Rows.Add(campo, estado, estado, usuario);
                                    }
                                    else
                                    {
                                        ObjTrazabilidad.Rows.Add(campo, valor, estado, usuario);
                                    }
                                }

                                //Valida Verificacion en Masa
                                if (campo == "Email" || campo == "Dir" || campo == "Nom1" || campo == "Nom2" || campo == "Ape1" || campo == "Ape2")
                                {
                                    if (estado == "0")
                                    {

                                        EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehValidationError);
                                    }
                                    else
                                    {
                                        EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                                    }
                                }
                                else
                                {
                                    if (EHFApp.Form.GetFormFieldNo(i).GetStatus() != ehFieldStatus.ehComplete)
                                    {
                                        EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehValidationError);
                                    }
                                }
                            }
                        }

                    }
                    else if (EHFApp.Job.GetName().ToUpper().Contains("SEGUNDA"))
                    {
                        for (short i = 1; i <= EHFApp.Form.GetMaxNoOfFormFields(); i++)
                        {
                            string campo = EHFApp.Form.GetFormFieldNo(i).GetName().Trim();
                            string valor = EHFApp.Form.GetFormFieldNo(i).GetValueStr().ToUpper().Trim();
                            string estado = EHFApp.Form.GetFormFieldNo(i).GetStatus() == ehFieldStatus.ehComplete ? "1" : "0";

#region Data
                            if (campo == "Nvlestudio" || campo == "Invidente" || campo == "Braile" || campo == "Genero" || campo == "Capacidad")
                            {
                                EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                                valor = ValidarCampos(campo, valor, ObjTrazabilidad);
                            }
                            if (campo == "cedula")
                            {
                                EHFApp.Form.GetFormFieldNo(i).SetValueStr("");
                            }
#endregion
                            if (valor != "%")
                            {
                                if (valor.Contains("*"))
                                {
                                    valor = "";
                                    EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                                    EHFApp.Form.GetFormFieldNo(i).SetValueStr(valor);
                                }
                                ObjTrazabilidad.Rows.Add(campo, valor, estado, usuario);
                            }
                            else if (valor == "%")
                            {
                                EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                            }


                        }
                    }
#endregion
                }

                //   string imagen = EHFApp.Form.GetSourceImage();

#region Proceso Normal
                if (EHFApp is InterpretApp)
                {
                    if (!EHFApp.Job.GetName().ToUpper().Contains("PRIMERA") && !EHFApp.Job.GetName().ToUpper().Contains("SEGUNDA"))
                    {
                        EHFApp.Form.SetStatus(ehFormStatus.ehComplete);
                        EHFApp.Form.SetQueues(0);
                        //(EHFApp as VerifyApp).SkipForm(1);
                    }
                    else
                    {
                        EHFApp.Form.SetQueues(1);
                    }
                }
                else
                {
                    if (EHFApp.Job.GetName().ToUpper().Contains("PRIMERA"))
                    {
                        //Valida Captura Preliminar -  false (proceso normal) - True (Es igual)


                        if (ValidarPrimeraCaptura2(ObjTrazabilidad, Cedula))
                        {
                            EHFApp.Form.SetQueues(2);

                        }
                        else
                        {
                            EHFApp.Form.SetStatus(ehFormStatus.ehComplete);
                            EHFApp.Form.SetQueues(0);
                        }

                    (EHFApp as VerifyApp).SkipForm(1);

                    }
                    else if (EHFApp.Job.GetName().ToUpper().Contains("SEGUNDA"))
                    {
                        EHFApp.Form.SetStatus(ehFormStatus.ehComplete);
                        EHFApp.Form.SetQueues(0);
                    }
                    else
                    {


                    }
                }

                if (completar) EHFApp.Form.SetStatus(ehFormStatus.ehComplete);
                ObjTrazabilidad.AcceptChanges();
                ExcuteTrazabilidad(Nombre, ObjTrazabilidad, proceso);

#endregion
                GC.Collect();

            }
            catch (Exception EX)
            {
                Log(EX.ToString());
                GC.Collect();
            }

        }


        private void VerificacionInterpretacionCamposRequeridos(string Formulario, string Cupon)
        {
            try
            {
                string valorCedula = this.EHFApp.Form.GetFormField("cedula", 0).GetValueStr().ToUpper();
                Dictionary<string, object> parametros = new Dictionary<string, object>();
                parametros.Add("@Formulario", Formulario);
                parametros.Add("@Orden", Cupon);
                DataTable dt = ExecuteSPCon(conexion, "[dbo].[Traking_Interpretacion_Validacion]", parametros);
                if (dt.Rows.Count > 0)
                {

                    string Cedula = dt.Rows[0]["Cedula"].ToString();
                    string Prinombre = dt.Rows[0]["Nombre1"].ToString();
                    string Segnombre = dt.Rows[0]["Nombre2"].ToString();
                    string Priapellido = dt.Rows[0]["Apellido1"].ToString();
                    string Segapellido = dt.Rows[0]["Apellido2"].ToString();
                    string Fexpedicion = dt.Rows[0]["Fexpedicion"].ToString();
                    string Anulado = dt.Rows[0]["Anulado"].ToString();
                    string Braille = dt.Rows[0]["Braille"].ToString();
                    string Capacidad = dt.Rows[0]["Capacidad"].ToString().ToUpper() == "S" ? "1" : "2";
                    string Genero = dt.Rows[0]["Genero"].ToString().ToUpper() == "M" ? "1" : "2";

                    string Invidente = dt.Rows[0]["Invidente"].ToString();
                    string Nvlestudio = dt.Rows[0]["Nvlestudio"].ToString();
                    string Dir = dt.Rows[0]["Dir"].ToString();
                    string Email = dt.Rows[0]["Email"].ToString();
                    string TelFijo = dt.Rows[0]["TelFijo"].ToString();
                    string TelMovil = dt.Rows[0]["TelMovil"].ToString();
                    string FechaInscripcion = dt.Rows[0]["FechaInscripcion"].ToString();

                    EstadoBloqueo = dt.Rows[0]["Bloqueado"].ToString().ToUpper() == "0" ? false : true;
                    EstadoNom1 = dt.Rows[0]["EstadoNom1"].ToString().ToUpper() == "0" ? false : true;
                    EstadoNom2 = dt.Rows[0]["EstadoNom2"].ToString().ToUpper() == "0" ? false : true;
                    EstadoApe1 = dt.Rows[0]["EstadoApe1"].ToString().ToUpper() == "0" ? false : true;
                    EstadoApe2 = dt.Rows[0]["EstadoApe2"].ToString().ToUpper() == "0" ? false : true;
                    EstadoFecha = dt.Rows[0]["EstadoFecha"].ToString().ToUpper() == "0" ? false : true;

                    string Dia = Fexpedicion.Substring(0, 2);
                    string Mes = Fexpedicion.Substring(3, 2);
                    string ano = Fexpedicion.Substring(6, 4);

                    string finsano = FechaInscripcion.Substring(9, 1);
                    string finsmes = FechaInscripcion.Substring(3, 2);
                    string finsdia = FechaInscripcion.Substring(0, 2);


#region Cedula
                    if (Cedula != "%")
                    {
                        EHFApp.Form.GetFormField("cedula", 0).SetValueStr(Cedula);
                        EHFApp.Form.GetFormField("cedula", 0).SetStatus(ehFieldStatus.ehRetype);
                    }
                    else
                    {
                        EHFApp.Form.GetFormField("cedula", 0).SetValueStr("");
                        EHFApp.Form.GetFormField("cedula", 0).SetStatus(ehFieldStatus.ehRetype);
                    }
#endregion
#region Primer nombre
                    if (Prinombre != "%")
                    {
                        EHFApp.Form.GetFormField("Nom1", 0).SetValueStr(Prinombre);
                        EHFApp.Form.GetFormField("Nom1", 0).SetStatus(ehFieldStatus.ehComplete);
                    }
                    else
                    {
                        EHFApp.Form.GetFormField("Nom1", 0).SetValueStr("");
                        EHFApp.Form.GetFormField("Nom1", 0).SetStatus(ehFieldStatus.ehRetype);
                    }
#endregion
#region Segundo nombre
                    if (Segnombre != "%")
                    {
                        EHFApp.Form.GetFormField("Nom2", 0).SetValueStr(Segnombre);
                        EHFApp.Form.GetFormField("Nom2", 0).SetStatus(ehFieldStatus.ehComplete);
                    }
                    else
                    {
                        EHFApp.Form.GetFormField("Nom2", 0).SetValueStr("");
                        EHFApp.Form.GetFormField("Nom2", 0).SetStatus(ehFieldStatus.ehRetype);
                    }
#endregion
#region primer nombre
                    if (Priapellido != "%")
                    {
                        EHFApp.Form.GetFormField("Ape1", 0).SetValueStr(Priapellido);
                        EHFApp.Form.GetFormField("Ape1", 0).SetStatus(ehFieldStatus.ehComplete);
                    }
                    else
                    {
                        EHFApp.Form.GetFormField("Ape1", 0).SetValueStr("");
                        EHFApp.Form.GetFormField("Ape1", 0).SetStatus(ehFieldStatus.ehRetype);
                    }
#endregion
#region segundo apellido
                    if (Segapellido != "%")
                    {
                        EHFApp.Form.GetFormField("Ape2", 0).SetValueStr(Segapellido);
                        EHFApp.Form.GetFormField("Ape2", 0).SetStatus(ehFieldStatus.ehComplete);
                    }
                    else
                    {
                        EHFApp.Form.GetFormField("Ape2", 0).SetValueStr("");
                        EHFApp.Form.GetFormField("Ape2", 0).SetStatus(ehFieldStatus.ehRetype);
                    }
#endregion
#region año
                    if (ano != "%")
                    {
                        EHFApp.Form.GetFormField("fexpeano", 0).SetValueStr(ano);
                        EHFApp.Form.GetFormField("fexpeano", 0).SetStatus(ehFieldStatus.ehComplete);
                    }
                    else
                    {
                        EHFApp.Form.GetFormField("fexpeano", 0).SetValueStr("");
                        EHFApp.Form.GetFormField("fexpeano", 0).SetStatus(ehFieldStatus.ehRetype);
                    }
#endregion
#region mes
                    if (Mes != "%")
                    {
                        EHFApp.Form.GetFormField("fexpemes", 0).SetValueStr(Mes);
                        EHFApp.Form.GetFormField("fexpemes", 0).SetStatus(ehFieldStatus.ehComplete);
                    }
                    else
                    {
                        EHFApp.Form.GetFormField("fexpemes", 0).SetValueStr("");
                        EHFApp.Form.GetFormField("fexpemes", 0).SetStatus(ehFieldStatus.ehRetype);
                    }
#endregion
#region Dia
                    if (Dia != "%")
                    {
                        EHFApp.Form.GetFormField("fexpedia", 0).SetValueStr(Dia);
                        EHFApp.Form.GetFormField("fexpedia", 0).SetStatus(ehFieldStatus.ehComplete);
                    }
                    else
                    {
                        EHFApp.Form.GetFormField("fexpedia", 0).SetValueStr("");
                        EHFApp.Form.GetFormField("fexpedia", 0).SetStatus(ehFieldStatus.ehInterpretError);
                    }
#endregion
#region Dir
                    if (Dir != "%")
                    {
                        EHFApp.Form.GetFormField("Dir", 0).SetValueStr(Dir);
                        EHFApp.Form.GetFormField("Dir", 0).SetStatus(ehFieldStatus.ehValidationError);
                    }
#endregion
#region TelFijo
                    if (TelFijo != "%")
                    {
                        EHFApp.Form.GetFormField("TelFijo", 0).SetValueStr(TelFijo);
                        EHFApp.Form.GetFormField("TelFijo", 0).SetStatus(ehFieldStatus.ehValidationError);
                    }
#endregion
#region TelMovil
                    if (TelMovil != "%")
                    {
                        EHFApp.Form.GetFormField("TelMovil", 0).SetValueStr(TelMovil);
                        EHFApp.Form.GetFormField("TelMovil", 0).SetStatus(ehFieldStatus.ehValidationError);
                    }
#endregion
#region Braille
                    if (Braille != "%")
                    {
                        EHFApp.Form.GetFormField("Braile", 0).SetValueStr(Braille);
                        EHFApp.Form.GetFormField("Braile", 0).SetStatus(ehFieldStatus.ehRetype);
                    }
#endregion
#region Invidente
                    if (Invidente != "%")
                    {
                        EHFApp.Form.GetFormField("Invidente", 0).SetValueStr(Invidente);
                        EHFApp.Form.GetFormField("Invidente", 0).SetStatus(ehFieldStatus.ehRetype);
                    }
#endregion
#region Nvlestudio
                    if (Nvlestudio != "%")
                    {
                        EHFApp.Form.GetFormField("Nvlestudio", 1).SetValueStr(Nvlestudio);
                        EHFApp.Form.GetFormField("Nvlestudio", 1).SetStatus(ehFieldStatus.ehRetype);
                        EHFApp.Form.GetFormField("Nvlestudio", 2).SetValueStr("");
                        EHFApp.Form.GetFormField("Nvlestudio", 2).SetStatus(ehFieldStatus.ehComplete);
                        EHFApp.Form.GetFormField("Nvlestudio", 3).SetValueStr("");
                        EHFApp.Form.GetFormField("Nvlestudio", 3).SetStatus(ehFieldStatus.ehComplete);
                        EHFApp.Form.GetFormField("Nvlestudio", 4).SetValueStr("");
                        EHFApp.Form.GetFormField("Nvlestudio", 4).SetStatus(ehFieldStatus.ehComplete);
                    }
#endregion
#region Genero
                    if (Genero != "%")
                    {
                        EHFApp.Form.GetFormField("Genero", 1).SetValueStr(Genero);
                        EHFApp.Form.GetFormField("Genero", 1).SetStatus(ehFieldStatus.ehRetype);
                        EHFApp.Form.GetFormField("Genero", 2).SetValueStr("");
                        EHFApp.Form.GetFormField("Genero", 2).SetStatus(ehFieldStatus.ehComplete);
                    }
#endregion
#region Capacidad
                    if (Capacidad != "%")
                    {
                        EHFApp.Form.GetFormField("Capacidad", 1).SetValueStr(Capacidad);
                        EHFApp.Form.GetFormField("Capacidad", 1).SetStatus(ehFieldStatus.ehRetype);
                        EHFApp.Form.GetFormField("Capacidad", 2).SetValueStr("");
                        EHFApp.Form.GetFormField("Capacidad", 2).SetStatus(ehFieldStatus.ehComplete);
                    }
#endregion
#region finsdia
                    if (finsdia != "%")
                    {
                        EHFApp.Form.GetFormField("finsdia", 0).SetValueStr(finsdia);
                        EHFApp.Form.GetFormField("finsdia", 0).SetStatus(ehFieldStatus.ehValidationError);
                    }
#endregion
#region finsmes
                    if (finsmes != "%")
                    {
                        EHFApp.Form.GetFormField("finsmes", 0).SetValueStr(finsmes);
                        EHFApp.Form.GetFormField("finsmes", 0).SetStatus(ehFieldStatus.ehValidationError);
                    }
#endregion
#region finsano
                    if (finsano != "%")
                    {
                        EHFApp.Form.GetFormField("finsano", 0).SetValueStr(finsano);
                        EHFApp.Form.GetFormField("finsano", 0).SetStatus(ehFieldStatus.ehValidationError);
                    }
#endregion
#region Email
                    if (Email != "%")
                    {
                        EHFApp.Form.GetFormField("Email", 0).SetValueStr(Email);
                        EHFApp.Form.GetFormField("Email", 0).SetStatus(ehFieldStatus.ehValidationError);
                    }
#endregion


                }
                else
                {
#region Primer nombre
                    EHFApp.Form.GetFormField("Nom1", 0).SetValueStr("");
                    EHFApp.Form.GetFormField("Nom1", 0).SetStatus(ehFieldStatus.ehRetype);
#endregion
#region Segundo nombre

                    EHFApp.Form.GetFormField("Nom2", 0).SetValueStr("");
                    EHFApp.Form.GetFormField("Nom2", 0).SetStatus(ehFieldStatus.ehRetype);

#endregion
#region primer nombre

                    EHFApp.Form.GetFormField("Ape1", 0).SetValueStr("");
                    EHFApp.Form.GetFormField("Ape1", 0).SetStatus(ehFieldStatus.ehRetype);

#endregion
#region segundo apellido

                    EHFApp.Form.GetFormField("Ape2", 0).SetValueStr("");
                    EHFApp.Form.GetFormField("Ape2", 0).SetStatus(ehFieldStatus.ehRetype);

#endregion
#region año

                    EHFApp.Form.GetFormField("fexpeano", 0).SetValueStr("");
                    EHFApp.Form.GetFormField("fexpeano", 0).SetStatus(ehFieldStatus.ehRetype);

#endregion
#region mes


                    EHFApp.Form.GetFormField("fexpemes", 0).SetValueStr("");
                    EHFApp.Form.GetFormField("fexpemes", 0).SetStatus(ehFieldStatus.ehRetype);

#endregion
#region Dia

                    EHFApp.Form.GetFormField("fexpedia", 0).SetValueStr("");
                    EHFApp.Form.GetFormField("fexpedia", 0).SetStatus(ehFieldStatus.ehInterpretError);

#endregion
#region Dir

                    EHFApp.Form.GetFormField("Dir", 0).SetValueStr("");
                    EHFApp.Form.GetFormField("Dir", 0).SetStatus(ehFieldStatus.ehValidationError);

#endregion
#region TelFijo

                    EHFApp.Form.GetFormField("TelFijo", 0).SetValueStr("");
                    EHFApp.Form.GetFormField("TelFijo", 0).SetStatus(ehFieldStatus.ehValidationError);

#endregion
#region TelMovil

                    EHFApp.Form.GetFormField("TelMovil", 0).SetValueStr("");
                    EHFApp.Form.GetFormField("TelMovil", 0).SetStatus(ehFieldStatus.ehValidationError);

#endregion
#region Braille

                    EHFApp.Form.GetFormField("Braile", 0).SetValueStr("");
                    EHFApp.Form.GetFormField("Braile", 0).SetStatus(ehFieldStatus.ehValidationError);

#endregion
#region Invidente

                    EHFApp.Form.GetFormField("Invidente", 0).SetValueStr("");
                    EHFApp.Form.GetFormField("Invidente", 0).SetStatus(ehFieldStatus.ehValidationError);

#endregion
#region Nvlestudio

                    EHFApp.Form.GetFormField("Nvlestudio", 1).SetValueStr("");
                    EHFApp.Form.GetFormField("Nvlestudio", 1).SetStatus(ehFieldStatus.ehValidationError);
                    EHFApp.Form.GetFormField("Nvlestudio", 2).SetValueStr("");
                    EHFApp.Form.GetFormField("Nvlestudio", 2).SetStatus(ehFieldStatus.ehComplete);
                    EHFApp.Form.GetFormField("Nvlestudio", 3).SetValueStr("");
                    EHFApp.Form.GetFormField("Nvlestudio", 3).SetStatus(ehFieldStatus.ehComplete);
                    EHFApp.Form.GetFormField("Nvlestudio", 4).SetValueStr("");
                    EHFApp.Form.GetFormField("Nvlestudio", 4).SetStatus(ehFieldStatus.ehComplete);

#endregion
#region Genero

                    EHFApp.Form.GetFormField("Genero", 1).SetValueStr("");
                    EHFApp.Form.GetFormField("Genero", 1).SetStatus(ehFieldStatus.ehValidationError);
                    EHFApp.Form.GetFormField("Genero", 2).SetValueStr("");
                    EHFApp.Form.GetFormField("Genero", 2).SetStatus(ehFieldStatus.ehComplete);

#endregion
#region Capacidad

                    EHFApp.Form.GetFormField("Capacidad", 1).SetValueStr("");
                    EHFApp.Form.GetFormField("Capacidad", 1).SetStatus(ehFieldStatus.ehValidationError);
                    EHFApp.Form.GetFormField("Capacidad", 2).SetValueStr("");
                    EHFApp.Form.GetFormField("Capacidad", 2).SetStatus(ehFieldStatus.ehComplete);


                    #endregion
                    #region finsdia

                    EHFApp.Form.GetFormField("finsdia", 0).SetValueStr("");
                    EHFApp.Form.GetFormField("finsdia", 0).SetStatus(ehFieldStatus.ehValidationError);

#endregion
#region finsmes

                    EHFApp.Form.GetFormField("finsmes", 0).SetValueStr("");
                    EHFApp.Form.GetFormField("finsmes", 0).SetStatus(ehFieldStatus.ehValidationError);

#endregion
#region finsano

                    EHFApp.Form.GetFormField("finsano", 0).SetValueStr("");
                    EHFApp.Form.GetFormField("finsano", 0).SetStatus(ehFieldStatus.ehValidationError);

#endregion
#region Email

                    EHFApp.Form.GetFormField("Email", 0).SetValueStr("");
                    EHFApp.Form.GetFormField("Email", 0).SetStatus(ehFieldStatus.ehRetype);

#endregion

#region Enmendadura

                    EHFApp.Form.GetFormField("enmenda", 0).SetValueStr("");
                    EHFApp.Form.GetFormField("enmenda", 0).SetStatus(ehFieldStatus.ehValidationError);

#endregion

#region Difhuella

                    EHFApp.Form.GetFormField("Difhuella", 0).SetValueStr("");
                    EHFApp.Form.GetFormField("Difhuella", 0).SetStatus(ehFieldStatus.ehValidationError);

#endregion

#region Huella

                    EHFApp.Form.GetFormField("Huellas", 0).SetValueStr("");
                    EHFApp.Form.GetFormField("Huellas", 0).SetStatus(ehFieldStatus.ehValidationError);

#endregion
                }
                EHFApp.Form.GetFormField("Anulado", 0).SetValueStr("No");
                EHFApp.Form.GetFormField("Anulado", 0).SetStatus(ehFieldStatus.ehValidationError);
                GC.Collect();
            }
            catch (Exception xe)
            {
                Log(xe.ToString());
                GC.Collect();
            }
        }

        private bool ValidarPrimeraCaptura2(DataTable dt, string Cedula)
        {
            try
            {

#region validacion
                bool respuesta = false;
                bool _Nvlestudio = false;
                bool _Genero = false;
                bool _Invidente = false;
                bool _Braile = false;
                bool _Discapacidad = false;

                string an = EHFApp.Form.GetFormField("Anulado", 0).GetValueStr();
                if (an == "SI")
                {
                    for (short i = 1; i <= EHFApp.Form.GetMaxNoOfFormFields(); i++)
                    {
                        EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehRetype);

                    }
                    EHFApp.Form.SetStatus(ehFormStatus.ehRetype);
                    respuesta = true;
                }
                else if (!Captura22)
                {
                    for (short i = 1; i <= EHFApp.Form.GetMaxNoOfFormFields(); i++)
                    {
                        string campo = EHFApp.Form.GetFormFieldNo(i).GetName().Trim();
                        switch (campo)
                        {
                            case "cedula":

                                // EHFApp.Form.GetFormFieldNo(i).SetValueStr("");
                                EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehRetype);
                                respuesta = true;
                                break;

                            case "Nom1":

                                //   EHFApp.Form.GetFormFieldNo(i).SetValueStr("");
                                EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehRetype);
                                respuesta = true;
                                break;

                            case "Nom2":

                                //  EHFApp.Form.GetFormFieldNo(i).SetValueStr("");
                                EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehRetype);
                                respuesta = true;
                                break;

                            case "Ape1":

                                // EHFApp.Form.GetFormFieldNo(i).SetValueStr("");
                                EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehRetype);
                                respuesta = true;
                                break;

                            case "Ape2":

                                //   EHFApp.Form.GetFormFieldNo(i).SetValueStr("");
                                EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehRetype);
                                respuesta = true;
                                break;

                            case "fexpeano":

                                //  EHFApp.Form.GetFormFieldNo(i).SetValueStr("");
                                EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehValidationError);
                                respuesta = true;
                                break;

                            case "fexpemes":

                                //    EHFApp.Form.GetFormFieldNo(i).SetValueStr("");
                                EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehValidationError);
                                respuesta = true;
                                break;

                            case "fexpedia":

                                //  EHFApp.Form.GetFormFieldNo(i).SetValueStr("");
                                EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehValidationError);
                                respuesta = true;
                                break;

                            case "Dir":

                                //   EHFApp.Form.GetFormFieldNo(i).SetValueStr("");
                                EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehValidationError);
                                respuesta = true;
                                break;

                            case "TelFijo":

                                //    EHFApp.Form.GetFormFieldNo(i).SetValueStr("");
                                EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehValidationError);
                                respuesta = true;
                                break;


                            case "TelMovil":

                                //    EHFApp.Form.GetFormFieldNo(i).SetValueStr("");
                                EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehValidationError);
                                respuesta = true;
                                break;

                            case "finsdia":

                                //  EHFApp.Form.GetFormFieldNo(i).SetValueStr("");
                                EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehValidationError);
                                respuesta = true;
                                break;

                            case "finsmes":

                                //  EHFApp.Form.GetFormFieldNo(i).SetValueStr("");
                                EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehValidationError);
                                respuesta = true;
                                break;

                            case "finsano":

                                //   EHFApp.Form.GetFormFieldNo(i).SetValueStr("");
                                EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehValidationError);
                                respuesta = true;
                                break;

                            case "Email":

                                //  EHFApp.Form.GetFormFieldNo(i).SetValueStr("");
                                EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehValidationError);
                                respuesta = true;
                                break;




                        }
                    }
                    EHFApp.Form.GetFormField("Anulado", 0).SetValueStr("");
                    EHFApp.Form.GetFormField("Anulado", 0).SetStatus(ehFieldStatus.ehRetype);

                    //EHFApp.Form.GetFormField("enmenda", 0).SetValueStr("");
                    //EHFApp.Form.GetFormField("enmenda", 0).SetStatus(ehFieldStatus.ehRetype);
                    //
                    //EHFApp.Form.GetFormField("enmenda", 0).SetValueStr("");
                    //EHFApp.Form.GetFormField("enmenda", 0).SetStatus(ehFieldStatus.ehRetype);

                    Captura22 = false;
                }
                else
                {
                    for (short i = 1; i <= EHFApp.Form.GetMaxNoOfFormFields(); i++)
                    {
                        EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                    }

                    EHFApp.Form.SetStatus(ehFormStatus.ehComplete);
                    EHFApp.Form.SetQueues(0);
                    (EHFApp as VerifyApp).SkipForm(1);
                }
#endregion
                GC.Collect();
                return respuesta;
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
                GC.Collect();
                return false;
            }
        }

        private bool ValidarPrimeraCaptura(DataTable dt, string Cedula)
        {
            try
            {
#region Variables
                bool respuesta = false;
                string Nombre = EHFApp.Form.GetEHKey();
                Dictionary<string, object> parametros = new Dictionary<string, object>();
                parametros.Add("@cedula", Cedula);
                DataTable Precargue = ExecuteSPCon(conexion, "[dbo].[Traking_Precargue_C]", parametros);

                String cedula = "", Nombre1 = "", Nombre2 = "", Apellido1 = "", Apellido2 = "", Direccion = "", TelefonoFijo = "", TelefonoMovil = "";
                String correoElectronico = "", Braille = "", invidente = "", FechaInscripcion = "", IdNivelAcademico = "", Genero = "", Discapacidad = "";

                String C_cedula = "", C_Nombre1 = "", C_Nombre2 = "", C_Apellido1 = "", C_Apellido2 = "", C_Direccion = "", C_TelefonoFijo = "", C_TelefonoMovil = "";
                String C_correoElectronico = "", C_Braille = "", C_invidente = "", C_FechaInscripcion = "", C_IdNivelAcademico = "", C_Genero = "", C_Discapacidad = "", C_FechExpe = "";
                string fexpedia_ = "";
                string fexpemes_ = "";
                string fexpeano_ = "";
                string finsdia_ = "";
                string finsmes_ = "";
                string finsano_ = "";
#endregion

#region Valores Pre cargue
                foreach (DataRow datos in Precargue.Rows)
                {
                    cedula = datos.ItemArray[1].ToString();
                    Nombre1 = datos.ItemArray[2].ToString();
                    Nombre2 = datos.ItemArray[3].ToString();
                    Apellido1 = datos.ItemArray[4].ToString();
                    Apellido2 = datos.ItemArray[5].ToString();
                    Direccion = datos.ItemArray[6].ToString();
                    TelefonoFijo = datos.ItemArray[7].ToString();
                    TelefonoMovil = datos.ItemArray[8].ToString();
                    correoElectronico = datos.ItemArray[9].ToString();
                    Braille = datos.ItemArray[10].ToString();
                    invidente = datos.ItemArray[11].ToString();
                    FechaInscripcion = datos.ItemArray[15].ToString();
                    IdNivelAcademico = datos.ItemArray[16].ToString();
                    Genero = datos.ItemArray[19].ToString();
                    Discapacidad = datos.ItemArray[20].ToString();
                }

#endregion

#region Valores Captura
                foreach (DataRow Info in dt.Rows)
                {
                    switch (Info.ItemArray[0].ToString())
                    {
                        case "cedula":
                            C_cedula = Info.ItemArray[1].ToString();
                            break;
                        case "Nom1":
                            C_Nombre1 = Info.ItemArray[1].ToString();
                            break;
                        case "Nom2":
                            C_Nombre2 = Info.ItemArray[1].ToString();
                            break;
                        case "Ape1":
                            C_Apellido1 = Info.ItemArray[1].ToString();
                            break;
                        case "Ape2":
                            C_Apellido2 = Info.ItemArray[1].ToString();
                            break;
                        case "Dir":
                            C_Direccion = Info.ItemArray[1].ToString();
                            break;
                        case "TelFijo":
                            C_TelefonoFijo = Info.ItemArray[1].ToString();
                            break;
                        case "TelMovil":
                            C_TelefonoMovil = Info.ItemArray[1].ToString();
                            break;
                        case "Email":
                            C_correoElectronico = Info.ItemArray[1].ToString();
                            break;
                        case "Braile":
                            C_Braille = Info.ItemArray[1].ToString();
                            break;
                        case "Invidente":
                            C_invidente = Info.ItemArray[1].ToString();
                            break;
                        case "Nvlestudio":
                            C_IdNivelAcademico = Info.ItemArray[1].ToString();
                            break;
                        case "Genero":
                            C_Genero = Info.ItemArray[1].ToString();
                            break;
                        case "fexpedia":
                            fexpedia_ = Info.ItemArray[1].ToString();
                            break;
                        case "fexpemes":
                            fexpemes_ = Info.ItemArray[1].ToString();
                            break;
                        case "fexpeano":
                            fexpeano_ = Info.ItemArray[1].ToString();
                            break;
                        case "finsdia":
                            finsdia_ = Info.ItemArray[1].ToString();
                            break;
                        case "finsmes":
                            finsmes_ = Info.ItemArray[1].ToString();
                            break;
                        case "finsano":
                            finsano_ = Info.ItemArray[1].ToString();
                            break;
                        case "Capacidad":
                            C_Discapacidad = Info.ItemArray[1].ToString();
                            break;
                    }



                }
                C_FechExpe = fexpedia_ + "//" + fexpemes_ + "//" + fexpeano_;
                C_FechaInscripcion = finsdia_ + "//" + finsmes_ + "//201" + finsano_;
#endregion

#region validacion
                bool _Nvlestudio = false;
                bool _Genero = false;
                bool _Invidente = false;
                bool _Braile = false;
                bool _Discapacidad = false;

                for (short i = 1; i <= EHFApp.Form.GetMaxNoOfFormFields(); i++)
                {
                    string campo = EHFApp.Form.GetFormFieldNo(i).GetName().Trim();
                    switch (campo)
                    {
                        case "cedula":
                            if (C_cedula != cedula)
                            {
                                EHFApp.Form.GetFormFieldNo(i).SetValueStr("");
                                EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehRetype);
                                respuesta = true;
                            }
                            else
                            {
                                EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                            }
                            break;

                        case "Nom1":
                            if (C_Nombre1.ToUpper() != Nombre1.ToUpper())
                            {
                                EHFApp.Form.GetFormFieldNo(i).SetValueStr("");
                                EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehRetype);
                                respuesta = true;
                            }
                            else
                            {
                                EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                            }
                            break;

                        case "Nom2":
                            if (C_Nombre2.ToUpper() != Nombre2.ToUpper())
                            {
                                EHFApp.Form.GetFormFieldNo(i).SetValueStr("");
                                EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehRetype);
                                respuesta = true;
                            }
                            else
                            {
                                EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                            }
                            break;
                        case "Ape1":
                            if (C_Apellido1.ToUpper() != Apellido1.ToUpper())
                            {
                                EHFApp.Form.GetFormFieldNo(i).SetValueStr("");
                                EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehRetype);
                                respuesta = true;
                            }
                            else
                            {
                                EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                            }
                            break;
                        case "Ape2":
                            if (C_Apellido2.ToUpper() != Apellido2.ToUpper())
                            {
                                EHFApp.Form.GetFormFieldNo(i).SetValueStr("");
                                EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehRetype);
                                respuesta = true;
                            }
                            else
                            {
                                EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                            }
                            break;
                        case "Dir":
                            if (C_Direccion.ToUpper() != Direccion.ToUpper())
                            {
                                EHFApp.Form.GetFormFieldNo(i).SetValueStr("");
                                EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehRetype);
                                respuesta = true;
                            }
                            else
                            {
                                EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                            }
                            break;
                        case "TelFijo":
                            if (C_TelefonoFijo != TelefonoFijo)
                            {
                                EHFApp.Form.GetFormFieldNo(i).SetValueStr("");
                                EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehRetype);
                                respuesta = true;
                            }
                            else
                            {

                                EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                            }
                            break;
                        case "TelMovil":
                            if (C_TelefonoMovil != TelefonoMovil)
                            {
                                EHFApp.Form.GetFormFieldNo(i).SetValueStr("");
                                EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehRetype);
                                respuesta = true;
                            }
                            else
                            {
                                EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                            }
                            break;
                        case "Email":
                            if (C_correoElectronico.ToUpper() != correoElectronico.ToUpper())
                            {
                                EHFApp.Form.GetFormFieldNo(i).SetValueStr("");
                                EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehRetype);
                                respuesta = true;
                            }
                            else
                            {
                                EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                            }
                            break;
                        case "Braile":
                            if ((C_Braille.ToUpper() == "" ? "0" : "1") != Braille)
                            {
                                if (!_Braile)
                                {
                                    EHFApp.Form.GetFormFieldNo(i).SetValueStr("");
                                    _Braile = true;
                                    EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehRetype);
                                }
                                else
                                {
                                    EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                                }
                                respuesta = true;
                            }
                            else
                            {
                                EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                            }
                            break;
                        case "Invidente":
                            if ((C_invidente.ToUpper() == "" ? "0" : "1") != invidente)
                            {
                                if (!_Invidente)
                                {
                                    EHFApp.Form.GetFormFieldNo(i).SetValueStr("");
                                    _Invidente = true;
                                    EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehRetype);
                                }
                                else
                                {
                                    EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                                }

                                respuesta = true;
                            }
                            else
                            {
                                EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                            }
                            break;
                        case "Nvlestudio":
                            if (C_IdNivelAcademico != IdNivelAcademico)
                            {
                                if (!_Nvlestudio)
                                {
                                    EHFApp.Form.GetFormFieldNo(i).SetValueStr("");
                                    EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehRetype);
                                    _Nvlestudio = true;
                                }
                                else
                                {
                                    EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                                }
                                respuesta = true;
                            }
                            else
                            {
                                EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                            }
                            break;
                        case "Genero":
                            if (C_Genero != (Genero.ToUpper() == "M" ? "1" : "2"))
                            {
                                if (!_Genero)
                                {
                                    EHFApp.Form.GetFormFieldNo(i).SetValueStr("");
                                    EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehRetype);
                                    _Genero = true;
                                }
                                else
                                {
                                    EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                                }
                                respuesta = true;
                            }
                            else
                            {
                                EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                            }
                            break;
                        case "fexpedia":
                            if (fexpedia_ != "")
                            {
                                EHFApp.Form.GetFormFieldNo(i).SetValueStr("");
                                EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehRetype);
                                respuesta = true;
                            }
                            else
                            {
                                EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                            }
                            break;
                        case "fexpemes":
                            if (fexpemes_ != "")
                            {
                                EHFApp.Form.GetFormFieldNo(i).SetValueStr("");
                                EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehRetype);
                                respuesta = true;
                            }
                            else
                            {
                                EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                            }
                            break;
                        case "fexpeano":
                            if (fexpeano_ != "")
                            {
                                EHFApp.Form.GetFormFieldNo(i).SetValueStr("");
                                EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehRetype);
                                respuesta = true;
                            }
                            else
                            {
                                EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                            }
                            break;

                        case "finsdia":
                            //  C_FechaInscripcion = Info.ItemArray[17].ToString() + "//" + Info.ItemArray[18].ToString() + "//201" + Info.ItemArray[19].ToString();
                            break;
                        case "Capacidad":
                            if (C_Discapacidad != (Discapacidad.ToUpper() == "N" ? "2" : "1"))
                            {
                                if (!_Discapacidad)
                                {
                                    EHFApp.Form.GetFormFieldNo(i).SetValueStr("");
                                    _Discapacidad = true;
                                    EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehRetype);

                                }
                                else
                                {
                                    EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                                }
                                respuesta = true;
                            }
                            else
                            {
                                EHFApp.Form.GetFormFieldNo(i).SetStatus(ehFieldStatus.ehComplete);
                            }
                            break;
                    }
                }
#endregion
                GC.Collect();
                return respuesta;
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
                GC.Collect();
                return false;
            }
        }

        private void ValidarDataRefence(string valor)
        {
            try
            {
                Dictionary<string, object> parametros = new Dictionary<string, object>();
                parametros.Add("@Cedula", valor);
                DataTable dt = ExecuteSPCon(conexion, "[dbo].[Traking_Get_DataReference]", parametros);

                if (dt.Rows.Count > 0)
                {
                    String Cedula = valor;
                    string Prinombre = dt.Rows[0]["Prinombre"].ToString();
                    string Segnombre = dt.Rows[0]["Segnombre"].ToString();
                    string Priapellido = dt.Rows[0]["Priapellido"].ToString();
                    string Segapellido = dt.Rows[0]["Segapellido"].ToString();
                    string Fexpedicion = dt.Rows[0]["Fexpedicion"].ToString();

                    string ano = Fexpedicion.Substring(6, 4);
                    string Mes = Fexpedicion.Substring(3, 2);
                    string Dia = Fexpedicion.Substring(0, 2);


                    EHFApp.Form.GetFormField("cedula", 0).SetValueStr(Cedula);
                    EHFApp.Form.GetFormField("cedula", 0).SetStatus(ehFieldStatus.ehRetype);

                    EHFApp.Form.GetFormField("Nom1", 0).SetValueStr(Prinombre);
                    EHFApp.Form.GetFormField("Nom1", 0).SetStatus(ehFieldStatus.ehRetype);

                    EHFApp.Form.GetFormField("Nom2", 0).SetValueStr(Segnombre);
                    EHFApp.Form.GetFormField("Nom2", 0).SetStatus(ehFieldStatus.ehRetype);

                    EHFApp.Form.GetFormField("Ape1", 0).SetValueStr(Priapellido);
                    EHFApp.Form.GetFormField("Ape1", 0).SetStatus(ehFieldStatus.ehRetype);

                    EHFApp.Form.GetFormField("Ape2", 0).SetValueStr(Segapellido);
                    EHFApp.Form.GetFormField("Ape2", 0).SetStatus(ehFieldStatus.ehRetype);

                    EHFApp.Form.GetFormField("fexpeano", 0).SetValueStr(ano);
                    EHFApp.Form.GetFormField("fexpeano", 0).SetStatus(ehFieldStatus.ehRetype);

                    EHFApp.Form.GetFormField("fexpemes", 0).SetValueStr(Mes);
                    EHFApp.Form.GetFormField("fexpemes", 0).SetStatus(ehFieldStatus.ehRetype);

                    EHFApp.Form.GetFormField("fexpedia", 0).SetValueStr(Dia);
                    EHFApp.Form.GetFormField("fexpedia", 0).SetStatus(ehFieldStatus.ehRetype);




                }
                GC.Collect();

            }
            catch (Exception ex)
            {
                Log(ex.ToString());
                GC.Collect();
            }
        }

        private string ValidarCampos(string campo, string valor, DataTable Tabla)
        {
            try
            {
                long validador;
                string resultado = "";

                switch (campo)
                {


                    case "Nvlestudio":

                        if (!ClassData.Nvlestudio)
                        {
                            resultado = GetMotivo("Nvlestudio", Tabla);
                            ClassData.Nvlestudio = true;
                        }
                        else
                        {
                            resultado = "%";
                        }

                        if (resultado == "")
                            ClassData.estado = "0";
                        else
                            ClassData.estado = "1";
                        GC.Collect();
                        return resultado;


                    case "Invidente":

                        if (!ClassData.Invidente)
                        {
                            resultado = GetMotivo("Invidente", Tabla);
                            ClassData.Invidente = true;
                        }
                        else
                        {
                            resultado = "%";
                        }

                        if (resultado == "")
                            ClassData.estado = "0";
                        else
                            ClassData.estado = "1";
                        GC.Collect();
                        return resultado;


                    case "Braile":

                        if (!ClassData.Braile)
                        {
                            resultado = GetMotivo("Braile", Tabla);
                            ClassData.Braile = true;
                        }
                        else
                        {
                            resultado = "%";
                        }

                        if (resultado == "")
                            ClassData.estado = "0";
                        else
                            ClassData.estado = "1";
                        GC.Collect();
                        return resultado;



                    case "Genero":

                        if (!ClassData.Genero)
                        {
                            resultado = GetMotivo("Genero", Tabla);
                            ClassData.Genero = true;
                        }
                        else
                        {
                            resultado = "%";
                        }

                        if (resultado == "")
                            ClassData.estado = "0";
                        else
                            ClassData.estado = "1";
                        GC.Collect();
                        return resultado;


                    case "Capacidad":

                        if (!ClassData.Capacidad)
                        {
                            resultado = GetMotivo("Capacidad", Tabla);
                            ClassData.Capacidad = true;
                        }
                        else
                        {
                            resultado = "%";
                        }

                        if (resultado == "")
                            ClassData.estado = "0";
                        else
                            ClassData.estado = "1";
                        GC.Collect();
                        return resultado;

                }
                GC.Collect();
                return resultado;

            }
            catch (Exception x)
            {
                Log(x.ToString());
                GC.Collect();
                return null;
            }
        }

        private string GetMotivo(string campo, DataTable Tabla)
        {
            try
            {
                string resultado = "";
                // string Nombre = @"C:\log\hola.txt"; 

                int num2 = 1;
                DataTable ObjBinarizacion = GetBinarizacion();
#region Recorre El campo de marcacion
                List<String> DatosEncontrados = new List<string>();
                for (short i = 1; i <= 8; i++)
                {
                    short LeftPos = 1;
                    short RightPos = 1;
                    short TopPos = 1;
                    short BottomPos = 1;
                    string imagen = EHFApp.Form.GetSourceImage();
                    string Linea = "";
                    if (EHFApp.Form.FormFieldNameExist(campo, i) == 1)
                    {
                        string text = this.EHFApp.Form.GetFormField(campo, i).GetValueStr().ToUpper();
                        if (text != "" && !text.Contains("*"))
                        {
                            DatosEncontrados.Add(text);
                            this.EHFApp.Form.GetFormField(campo, i).GetBinaryImage(ref LeftPos, ref RightPos, ref TopPos, ref BottomPos);
                            ObjBinarizacion.Rows.Add(campo, i.ToString(), LeftPos.ToString(), RightPos.ToString(), TopPos.ToString(), BottomPos.ToString());
                        }


                        num2++;
                    }


                }
#endregion



                if (DatosEncontrados.Count == 1)
                {
                    foreach (string ValorEscogido in DatosEncontrados)
                    {
                        resultado = ValorEscogido;
                    }
                }
                else if (DatosEncontrados.Count == 0)
                {
                    resultado = "";
                }
                else
                {
                    string Veredicto = ValidarMultiMarca(ObjBinarizacion, Tabla);
                    if (Veredicto == "0" || Veredicto == "")
                    {
                        resultado = "4";
                    }
                    else
                    {
                        resultado = Veredicto;
                    }
                }
                GC.Collect();
                return resultado;
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
                GC.Collect();
                return null;
            }
        }

        private DataTable GetBinarizacion()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Campo", typeof(string));
            dt.Columns.Add("Posicion", typeof(string));
            dt.Columns.Add("LeftPos", typeof(string));
            dt.Columns.Add("RightPos", typeof(string));
            dt.Columns.Add("TopPos", typeof(string));
            dt.Columns.Add("BottomPos", typeof(string));
            GC.Collect();
            return dt;
        }

        private string ValidarMultiMarca(DataTable ObjBinarizacion, DataTable Tabla)
        {
            try
            {
                string resultado = "";
                int ValorMayor = 0;
                int Posicion = 0;
                int Mes = DateTime.Now.Month;
                int Dia = DateTime.Now.Day;



                foreach (DataRow Dato in ObjBinarizacion.Rows)
                {
#region variables
                    string V_Campo = Dato.ItemArray[0].ToString();
                    string V_Posicion = Dato.ItemArray[1].ToString();
                    string V_LeftPos = Dato.ItemArray[2].ToString();
                    string V_RightPos = Dato.ItemArray[3].ToString();
                    string V_TopPos = Dato.ItemArray[4].ToString();
                    string V_BottomPos = Dato.ItemArray[5].ToString();
#endregion



                    if (ValorMayor == 0)
                    {
                        ValorMayor = int.Parse(V_RightPos) * int.Parse(V_BottomPos);
                        Posicion = int.Parse(V_Posicion);
                    }
                    else
                    {

                        int ValorMayor_Temp = int.Parse(V_RightPos) * int.Parse(V_BottomPos);
                        if (ValorMayor_Temp > ValorMayor)
                        {
                            Posicion = int.Parse(V_Posicion);
                        }


                    }




                }
                GC.Collect();
                return Posicion.ToString();
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
                GC.Collect();
                return null;
            }
        }

        private int ValidarMaxDia(int Mes, int MesSelecionado)
        {
            try
            {
                int respuesta = 0;
                int Dia = DateTime.Now.Day;

                if (Mes == MesSelecionado)
                {
                    respuesta = Dia;
                }
                else
                {
                    respuesta = 31;
                }
                GC.Collect();
                return respuesta;
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
                GC.Collect();
                return 0;
            }
        }

        private void HabilitarPrimerCampoMarcacion(string campo)
        {

            try
            {
                for (short i = 0; i <= 8; i++)
                {
                    if (EHFApp.Form.FormFieldNameExist(campo, i) == 1)
                    {
                        if (i == 1)
                        {
                            EHFApp.Form.GetFormField(campo, i).SetValueStr("");
                            EHFApp.Form.GetFormField(campo, i).SetStatus(ehFieldStatus.ehInterpretError);
                        }
                        else
                        {
                            EHFApp.Form.GetFormField(campo, i).SetValueStr("");
                            EHFApp.Form.GetFormField(campo, i).SetStatus(ehFieldStatus.ehComplete);
                        }

                    }
                }
                GC.Collect();
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
                GC.Collect();

            }

        }

        private string ValidadarCamposMarcacion(string campo, string Longitud)
        {
            try
            {

                int num2 = 1;
                String Valor = "1";
                String vlr_Escogido = "";
#region Recorre El campo de marcacion
                List<String> DatosEncontrados = new List<string>();
                for (short i = 1; i <= int.Parse(Longitud); i++)
                {
                    if (EHFApp.Form.FormFieldNameExist(campo, i) == 1)
                    {
                        string text = this.EHFApp.Form.GetFormField(campo, i).GetValueStr().ToUpper();
                        if (text != "")
                        {
                            DatosEncontrados.Add(text);
                            //resulto = text;
                            //num++;
                            //num3 = num2;
                        }
                        num2++;
                    }
                }
#endregion
                ClassData.DatosEncontradosP = DatosEncontrados;

#region Limpia campos
                for (short j = 1; j <= int.Parse(Longitud); j += 1)
                {
                    if (EHFApp.Form.FormFieldNameExist(campo, j) == 1)
                    {
                        EHFApp.Form.GetFormField(campo, j).SetStatus(ehFieldStatus.ehComplete);
                        this.EHFApp.Form.GetFormField(campo, j).SetValueStr(string.Empty);
                    }
                }
#endregion

#region  pregunta si tiene 1 opcion escogida
                if (DatosEncontrados.Count == 1)
                {

                    foreach (string ValorEscogido in DatosEncontrados)
                    {
                        vlr_Escogido = ValorEscogido;
                    }
                    this.EHFApp.Form.GetFormField(campo, 1).SetValueStr(vlr_Escogido);
                }
                else
                {
                    EHFApp.Form.GetFormField(campo, 1).SetStatus(ehFieldStatus.ehInterpretError);
                    Valor = DatosEncontrados.Count.ToString();
                }

                ClassData.VlrCaptura = Valor;
#endregion
                GC.Collect();
                return vlr_Escogido;
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
                GC.Collect();
                return "";


            }

        }

        private bool ValidarDataTable(string campo, string valor, DataTable ObjTrazabilidad)
        {
            try
            {

                foreach (DataRow Dato in ObjTrazabilidad.Rows)
                {

                    string campo2 = Dato.ItemArray[0].ToString();
                    string Vlr2 = Dato.ItemArray[1].ToString();
                    if (campo == campo2 && valor == "")
                    {
                        GC.Collect();
                        return false;
                    }
                }
                GC.Collect();
                return true;
            }
            catch
            {
                GC.Collect();
                return false;
            }
        }

        private bool GetValidation(string campo, string valor, int proceso)
        {
            try
            {
                if (proceso == 1)
                {
                    Dictionary<string, object> parametros = new Dictionary<string, object>();
                    parametros.Add("Name", campo);
                    DataTable dt = ExecuteSPCon(conexion, "[dbo].[GetValidation]", parametros);

                    if (dt.Rows.Count > 0)
                    {

                        foreach (DataRow Dato in dt.Rows)
                        {
                            string[] Prohibitedvalues = Dato.ItemArray[3].ToString().Split(';');
                            foreach (string Prohibidos in Prohibitedvalues)
                            {
#region Valida Valores Prohibidos
                                if (Prohibidos == valor)
                                {
                                    return false;
                                }
#endregion

#region Valida Logitud
                                int Longitud = int.Parse(Dato.ItemArray[4].ToString());
                                if (!campo.ToUpper().Contains("COD"))
                                {
                                    if (Longitud < valor.Length)
                                    {
                                        GC.Collect();
                                        return false;
                                    }
                                }
                                else
                                {
                                    GC.Collect();
                                    return true;
                                }
#endregion

                            }
                        }

                        GC.Collect();
                        return true;
                    }
                    else
                    {
                        GC.Collect();
                        return true;
                    }
                }
                else
                {
                    GC.Collect();
                    return true;
                }
            }
            catch
            {
                GC.Collect();
                return false;
            }
        }

        private void GuardaVlrNumeracion(string Resultado, string Valor)
        {
            try
            {
                //for (short i = 1; i <= 8; i++)
                //{
                //if (EHFApp.Form.FormFieldNameExist("Numeracion"+i, 0) == 1)
                //{
                EHFApp.Form.GetFormField(Resultado, 0).SetValueStr(Valor);
                EHFApp.Form.GetFormField(Resultado, 0).SetStatus(ehFieldStatus.ehComplete);
                //}
                // }
                GC.Collect();
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
                GC.Collect();
            }
        }

        private int ValidarCamposDentroValores(string campo, string valor)
        {
            try
            {
#region Variables
                List<string> Lista = new List<string>();
                List<string> ListaFecha = new List<string>();

#endregion

#region Valor indexacion

                Lista.Add(valor);

#endregion

#region Valor para  Marca Fecha
                for (short num = 1; num <= 8; num++)
                {
                    string text = this.EHFApp.Form.GetFormField(campo, num).GetValueStr().ToUpper();
                    if (text != "")
                    {
                        ListaFecha.Add(text);
                    }
                }
#endregion


#region Validar Informacion Marcha Fecha
                if (campo == "Marca_fecha")
                {
                    if (!ListaFecha.Contains(Lista[0].ToString()))
                    {
                        for (short j = 1; j <= 8; j += 1)
                        {
                            if (EHFApp.Form.FormFieldNameExist(campo, j) == 1)
                            {
                                if (j == 1)
                                {
                                    EHFApp.Form.GetFormField(campo, j).SetStatus(ehFieldStatus.ehInterpretError);
                                }
                                else
                                {
                                    EHFApp.Form.GetFormField(campo, j).SetStatus(ehFieldStatus.ehComplete);
                                }
                                this.EHFApp.Form.GetFormField(campo, j).SetValueStr(string.Empty);
                            }
                        }
                        GC.Collect();
                        return 1;
                    }
                    else
                    {
                        GC.Collect();
                        return 2;
                    }
                }
#endregion


                GC.Collect();
                return 1;
            }
            catch
            {
                GC.Collect();
                return 2;
            }
        }

        private int ValidarCamposMarcacion(string campo, int proceso)
        {
            int result;
            try
            {
                int num = 0;
                int num2 = 1;
                int num3 = 0;
                string resulto = "";
                for (short i = 1; i <= 8; i++)
                {
                    if (EHFApp.Form.FormFieldNameExist(campo, i) == 1)
                    {

                        string text = this.EHFApp.Form.GetFormField(campo, i).GetValueStr().ToUpper();
                        if (text != "")
                        {
                            resulto = text;
                            num++;
                            num3 = num2;
                        }
                        num2++;
                    }
                }
#region Proceso 1
                if (proceso == 1)
                {
                    if (num != 1)
                    {
                        for (short j = 1; j <= 8; j += 1)
                        {
                            if (EHFApp.Form.FormFieldNameExist(campo, j) == 1)
                            {
                                if (j == 1)
                                {
                                    EHFApp.Form.GetFormField(campo, j).SetStatus(ehFieldStatus.ehInterpretError);
                                }
                                this.EHFApp.Form.GetFormField(campo, j).SetValueStr(string.Empty);
                            }
                        }
                    }
                }
#endregion

                if (campo == "MarcaMotivo")
                {
                    if (resulto == "")
                    {
                        num3 = 0;
                    }
                    else
                    {
                        num3 = int.Parse(resulto);
                    }
                }
                result = num3;
            }
            catch
            {
                GC.Collect();
                result = 99;
            }
            GC.Collect();
            return result;
        }

        private string ValidarVerify(string campo)
        {
            string result;
            try
            {
                string text = "";
                string b = "";
                for (short num = 1; num <= 8; num += 1)
                {
                    text = this.EHFApp.Form.GetFormField(campo, num).GetValueStr().ToUpper();
                    if (text != "")
                    {
                        b = text;
                    }
                }
                for (short num = 1; num <= 8; num += 1)
                {
                    text = this.EHFApp.Form.GetFormField("F_Ps" + num, 0).GetValueStr().ToUpper();
                    if (text == b)
                    {
                        EHFApp.Form.GetFormField(campo, 1).SetValueStr(num.ToString());
                        EHFApp.Form.GetFormField(campo, 1).SetStatus(ehFieldStatus.ehComplete);
                        result = num.ToString();
                        GC.Collect();
                        return result;
                    }
                }
                result = text;
            }
            catch (Exception var_3_FE)
            {
                Log(var_3_FE.ToString());
                result = null;
                GC.Collect();

            }
            GC.Collect();
            return result;
        }

        private int GuardaVlr2Captura(string campo)
        {
            try
            {

                string text = "";
                List<string> list = new List<string>();
                //-------------------------------------ValidarCamposDentroValores de F_Ps
                for (short i = 1; i <= 8; i += 1)
                {
                    if (EHFApp.Form.FormFieldNameExist("F_Ps" + i, 0) == 1)
                    {
                        text = EHFApp.Form.GetFormField("F_Ps" + i, 0).GetValueStr().ToUpper();
                        if (text != "")
                        {
                            list.Add(text);
                        }
                    }
                }


                GC.Collect();
                return 1;
            }

            catch (Exception ezx)
            {
                Log(ezx.ToString());
                GC.Collect();
                GC.Collect();
                return 2;

            }
        }

        private void GuardaVlr(string campo)
        {
            try
            {
                int num = 0;
                int num2 = 0;
                int num3 = 1;
                string text = "";
                List<string> list = new List<string>();
                int num4 = int.Parse(campo.Substring(4, 1));
                string ValorInical = "";
                long num5 = 0;
                long numerico = 0;
                string NuevoCampo = campo.Substring(0, campo.Length - 1);

                if (!long.TryParse(ValorInical, out num5))
                {
                    for (short num6 = 1; num6 <= 8; num6 += 1)
                    {
                        if (EHFApp.Form.FormFieldNameExist(NuevoCampo + num6, 0) == 1)
                        {
                            text = EHFApp.Form.GetFormField(NuevoCampo + num6, 0).GetValueStr().ToUpper();
                            if (text != "")
                            {
                                list.Add(text);
                            }
                        }
                    }
                    foreach (string current in list)
                    {
                        long num7 = 0;
                        if (long.TryParse(current.Replace("*", ""), out num7))
                        {
                            if (num == 0)
                            {
                                if (current.Length > 2)
                                {
                                    num = int.Parse(current.Replace("*", "").Substring(0, 2));
                                }
                                else
                                {
                                    num = int.Parse(current.Replace("*", ""));
                                }
                                num2 = num3;
                            }
                            else
                            {
                                int num8 = int.Parse(current.Replace("*", ""));
                                int num9 = num3;
                                int num10 = num + (num9 - num2);
                                if (num10 == num8)
                                {
                                    text = num.ToString();
                                    break;
                                }
                            }
                        }
                        num3++;
                    }
                }
                if (text.Contains("*"))
                {
                    if (num != 0)
                    {
                        text = (num + 1 - (num2 - num4)).ToString();
                        if (int.Parse(text) > 0 && int.Parse(text) <= 31)
                        {
                            text = text;
                        }
                        else
                        {
                            text = "1";
                        }
                    }
                    else
                    {
                        text = "1";
                    }
                }
                else if (text == "0")
                {
                    text = "1";
                }
                else if (long.TryParse(text.Replace("*", ""), out numerico))
                {
                    if (int.Parse(text) > 31)
                    {
                        text = "1";
                    }
                    else
                    {
                        int Posicion = 0;
                        int Cposi = 1;
                        foreach (string Valor in list)
                        {
                            if (Valor == text)
                            {
                                Posicion = Cposi;
                                int valorOrigen = int.Parse(text);
                                int PosicionInicial = num4;
                                text = (valorOrigen - (Posicion - PosicionInicial)).ToString();

                            }
                            Cposi++;
                        }
                    }
                }

                string valueStr = "";
                for (short num6 = 1; num6 <= 8; num6 += 1)
                {
                    if (num4 < (int)num6)
                    {
                        valueStr = (int.Parse(text) - (num4 - (int)num6)).ToString();
                    }
                    if (num4 > (int)num6)
                    {
                        valueStr = (int.Parse(text) + ((int)num6 - num4)).ToString();
                    }
                    if (num4 == (int)num6)
                    {
                        valueStr = text;
                    }
                    EHFApp.Form.GetFormField("F_Ps" + num6, 0).SetValueStr(valueStr);
                    EHFApp.Form.GetFormField("F_Ps" + num6, 0).SetStatus(ehFieldStatus.ehComplete);
                }
                GC.Collect();
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
                GC.Collect();
            }
        }

        private DataTable GetDataTableBasico()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Campo", typeof(string));
            dt.Columns.Add("Valor", typeof(string));
            dt.Columns.Add("Aprobado", typeof(int));
            //  dt.Columns.Add("VlrCapturado", typeof(string));
            dt.Columns.Add("Usuario", typeof(string));
            GC.Collect();
            return dt;
        }

        private void InsertIngreso(string nombreSet, string setDef, string nombreDoc, string docDef, string imagen, string usuario)
        {
            Dictionary<string, object> parametros = new Dictionary<string, object>();
            parametros.Add("nombreSet", nombreSet);
            parametros.Add("setDef", setDef);
            parametros.Add("nombreDoc", nombreDoc);
            parametros.Add("docDef", docDef);
            parametros.Add("imagen", imagen);
            parametros.Add("usuario", usuario);
            ExecuteSPCon(conexion, "[dbo].[Traking_Ingreso]", parametros);
            GC.Collect();
        }

        public string ExportImg(string fileName)
        {
            string RDestino = "";
            try
            {
                FileInfo f = new FileInfo(fileName);

                Dictionary<string, object> parametros = new Dictionary<string, object>();
                parametros.Add("@Name", "RutaExportImg");
                parametros.Add("@opcion", "1");
                DataTable Ruta = ExecuteSPCon(conexion, "[dbo].[Get_Traking_Set]", parametros);

                RDestino = Ruta.Rows[0][0].ToString() + f.Name.Replace(f.Extension, "") + ".JPG";

                Bitmap bmp1 = new Bitmap(fileName);
                ImageCodecInfo jgpEncoder = GetEncoder(ImageFormat.Jpeg);
                System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
                EncoderParameters myEncoderParameters = new EncoderParameters(1);
                EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 4L);
                myEncoderParameters.Param[0] = myEncoderParameter;
                bmp1.Save(RDestino, jgpEncoder, myEncoderParameters);

                bmp1.Dispose();
                GC.Collect();
                return RDestino;

            }
            catch (Exception x)
            {
                Log(x.ToString());
                GC.Collect();
                return RDestino;
            }

        }

        private void InsertSalida(string nombre, string imagen, string usuario)
        {
            try
            {

                string RutaExporImg = ExportImg(imagen);


#region BD
                Dictionary<string, object> parametros = new Dictionary<string, object>();
                parametros.Add("nombre", nombre);
                parametros.Add("imagen", RutaExporImg);
                parametros.Add("usuario", usuario);
                DataTable resp = ExecuteSPCon(conexion, "[dbo].[Traking_Salida]", parametros);
#endregion

#region Cupon
                Dictionary<string, object> parametros2 = new Dictionary<string, object>();
                parametros2.Add("@Name", "RutaExport");
                parametros2.Add("@opcion", "1");
                DataTable Ruta = ExecuteSPCon(conexion, "[dbo].[Get_Traking_Set]", parametros2);
                string RutaCupon = Ruta.Rows[0][0].ToString();
#endregion

#region Formulario
                Dictionary<string, object> parametros3 = new Dictionary<string, object>();
                parametros3.Add("@Name", "RutaExportFormulario");
                parametros3.Add("@opcion", "1");
                DataTable Ruta3 = ExecuteSPCon(conexion, "[dbo].[Get_Traking_Set]", parametros3);
                string RutaFormulario = Ruta3.Rows[0][0].ToString();
#endregion

#region Huella
                Dictionary<string, object> parametros4 = new Dictionary<string, object>();
                parametros4.Add("@Name", "RutaExportHuella");
                parametros4.Add("@opcion", "1");
                DataTable Ruta4 = ExecuteSPCon(conexion, "[dbo].[Get_Traking_Set]", parametros4);
                string RutaHuella = Ruta4.Rows[0][0].ToString();
#endregion

                String Formulario = resp.Rows[0]["NumeroCodBar"].ToString();
                String cupon = resp.Rows[0]["cupon"].ToString().Replace("Cupon", "");
                String cedula = resp.Rows[0]["NoIdent"].ToString();
                String departamento = resp.Rows[0]["Depto"].ToString();
                String municipio = resp.Rows[0]["Municipio"].ToString();
                String puesto = resp.Rows[0]["Puesto"].ToString();
                String Ubicacion = resp.Rows[0]["Ubicacion"].ToString();
                String Form = resp.Rows[0]["RutaFormulario"].ToString();

                FileInfo Dir = new FileInfo(Ubicacion);
                RutaHuella = RutaHuella + "\\" + departamento + "\\" + municipio + "\\" + puesto + "\\";
                RutaCupon = RutaCupon + departamento + "\\" + municipio + "\\" + puesto + "\\";
                RutaFormulario = RutaFormulario + departamento + "\\" + municipio + "\\" + puesto + "\\";

                DirectoryInfo DirHUella = new DirectoryInfo(RutaHuella);
                DirectoryInfo DirCupon = new DirectoryInfo(RutaCupon);
                DirectoryInfo DirFormulario = new DirectoryInfo(RutaFormulario);

                if (!DirHUella.Exists)
                    DirHUella.Create();

                if (!DirCupon.Exists)
                    DirCupon.Create();

                if (!DirFormulario.Exists)
                    DirFormulario.Create();


                string NOmbre = Formulario + "-" + cupon + "-" + cedula + "-" + departamento + municipio + puesto + ".Bmp";
                string NombreCupon = departamento + municipio + puesto + "-" + Formulario + "-" + cupon + ".JPG";
                string NombreFormulario = departamento + municipio + puesto + "-" + Formulario + ".TIFF";
                string NombreFormulario2 = departamento + municipio + puesto + "-" + Formulario + ".JPG";

                //huella
                //   File.Copy(Ubicacion, RutaHuella + NOmbre, true);
                //Cupon
                //   File.Copy(RutaExporImg, RutaCupon + NombreCupon, true);
                GuardarImagen(200, RutaExporImg, RutaCupon + NombreCupon);
                //formulario
                RecortarImagen(200, Form, RutaFormulario + NombreFormulario);
                //formulario JPG
                RecortarImagen(500, RutaFormulario + NombreFormulario, RutaFormulario + NombreFormulario2);

                ConsumimirAPI(resp);
                // EscribirArchivoPlano(resp);
                // EscribirArchivoPlanoCSV(resp);
                GC.Collect();

            }
            catch (Exception e)
            {
                Log(e.ToString());
                MessageBox.Show(e.Message);
                GC.Collect();

            }
        }

        private void ConsumimirAPI(DataTable resp)
        {
            try
            {
                String Formulario = resp.Rows[0]["NumeroCodBar"].ToString();
                String cupon = resp.Rows[0]["cupon"].ToString().Replace("Cupon", "");
                String cedula = resp.Rows[0]["NoIdent"].ToString();
                String Form = resp.Rows[0]["CuponRuta"].ToString();

                String Nombre1 = resp.Rows[0]["Nombre1"].ToString();
                String Nombre2 = resp.Rows[0]["Nombre2"].ToString();
                String Apellido1 = resp.Rows[0]["Apellido1"].ToString();
                String Apellido2 = resp.Rows[0]["Apellido2"].ToString();
                String Direccion = resp.Rows[0]["Direccion"].ToString();
                String TelFijo = resp.Rows[0]["TelFijo"].ToString();
                String TelMovil = resp.Rows[0]["TelMovil"].ToString();
                String Email = resp.Rows[0]["Email"].ToString();
                String Braile = resp.Rows[0]["Braile"].ToString();
                String Invidente = resp.Rows[0]["Invidente"].ToString();
                String NiveldeEstudio = resp.Rows[0]["NiveldeEstudio"].ToString();
                String Genero = resp.Rows[0]["Genero"].ToString();
                String FechaExpedicion = resp.Rows[0]["FechaExpedicion"].ToString();
                String FechaInscripcion = resp.Rows[0]["FechaInscripcion"].ToString();
                String Discapacidad = resp.Rows[0]["Discapacidad"].ToString();
                String Fullpath = resp.Rows[0]["Fullpath"].ToString();
                String Novedades = resp.Rows[0]["Novedades"].ToString();


                var httpWebRequest = (HttpWebRequest)WebRequest.Create(sUrlRequest);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";
                var Documentos = new List<Documento>();

#region Estructura
                var Estructura = new List<Estructura>();


                String Resultado = CrearKeyword(1, 22, "formulario", Formulario, 1);
                Resultado = Resultado + "," + CrearKeyword(5, 23, "cedula", cedula, 1);
                Resultado = Resultado + "," + CrearKeyword(1, 24, "Nom1", Nombre1, 1);
                Resultado = Resultado + "," + CrearKeyword(1, 25, "Nom2", Nombre2, 1);
                Resultado = Resultado + "," + CrearKeyword(1, 26, "Ape1", Apellido1, 1);
                Resultado = Resultado + "," + CrearKeyword(1, 27, "Ape2", Apellido2, 1);
                Resultado = Resultado + "," + CrearKeyword(1, 28, "Dir", Direccion, 1);
                Resultado = Resultado + "," + CrearKeyword(1, 29, "TelFijo", TelFijo, 1);
                Resultado = Resultado + "," + CrearKeyword(1, 30, "TelMovil", TelMovil, 1);
                Resultado = Resultado + "," + CrearKeyword(1, 31, "Email", Email, 1);
                Resultado = Resultado + "," + CrearKeyword(1, 32, "Braile", Braile, 1);
                Resultado = Resultado + "," + CrearKeyword(1, 33, "Invidente", Invidente, 1);
                Resultado = Resultado + "," + CrearKeyword(1, 36, "Nvlestudio", NiveldeEstudio, 1);
                Resultado = Resultado + "," + CrearKeyword(1, 37, "Genero", Genero, 1);
                Resultado = Resultado + "," + CrearKeyword(1, 38, "FechaExpedicion", FechaExpedicion, 1);
                Resultado = Resultado + "," + CrearKeyword(1, 39, "FechaInscripcion", FechaInscripcion, 1);
                Resultado = Resultado + "," + CrearKeyword(1, 40, "Capacidad", Discapacidad, 1);
                Resultado = Resultado + "," + CrearKeyword(1, 41, "Cupon", cupon, 1);
                Resultado = Resultado + "," + CrearKeyword(1, 42, "Estado", "CAPTURADO", 1);
                if (Novedades != "")
                    Resultado = Resultado + "," + CrearKeyword(1, 43, "Novedad", Novedades, 1);

                Resultado = "[" + Resultado + "]";

#endregion

#region body
                String Doc = CrearDocumento(27, 1, 1, Resultado, Form.Replace("\\", "//"));
#endregion

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(Doc);
                    streamWriter.Flush();
                }
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                }
                GC.Collect();
            }
            catch (Exception x)
            {
                Log(x.ToString());
                MessageBox.Show(x.Message);
                GC.Collect();
            }
        }

        public class Credenciales
        {
            public string email { get; set; }
            public string password { get; set; }
        }

        public class Documento
        {
            public int Doctype { get; set; }
            public int User { get; set; }
            public double Status { get; set; }
            public string Keywords { get; set; }
        }

        public class Estructura
        {
            public string Type { get; set; }
            public string Id { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }
            public string Section { get; set; }
        }

        private string CrearKeyword(int Type, int Id, string Name, string Value, int Section)
        {
            //"Id":"1","Name":"TExtension","Value":"txt","Section":"1"}
            string obj = "{" + "'Type':'" + Type + "'," +
                                "'Id':" + Id + "," +
                                "'Name':'" + Name + "'," +
                                "'Value':'" + Value + "'," +
                                "'Section':" + Section + "}";

            GC.Collect();
            return obj;
        }

        private string CrearDocumento(int Doctype, int User, int Status, string Resultado, string Imagen)
        {

            string tra = "{" + (char)(34) + "Doctype" + (char)(34) + ":" + Doctype + "," +
                                (char)(34) + "Status" + (char)(34) + ":" + Status + "," +
                                (char)(34) + "User" + (char)(34) + ":" + User + "," +
                                (char)(34) + "Keywords" + (char)(34) + ":" + (char)(34) + Resultado + (char)(34) + "," +
                                (char)(34) + "File" + (char)(34) + ":" + (char)(34) + Imagen + (char)(34) + "}";
            GC.Collect();
            return tra;
        }

        private void RecortarImagen(int dpi, string RutaInicial, string RutaFinal)
        {
            try
            {

                using (Bitmap newBitmap = new Bitmap(RutaInicial))
                {

                    // ----------------------------------------------------

                    System.Drawing.Imaging.Encoder myEncoder; EncoderParameter myEncoderParameter; EncoderParameters myEncoderParameters;
                    myEncoder = System.Drawing.Imaging.Encoder.Compression;

                    ImageCodecInfo myImageCodecInfo;
                    myImageCodecInfo = GetEncoderInfo("image/tiff");
                    Image imageIn = Image.FromFile(RutaInicial);
                    //  Bitmap bmPhoto = new Bitmap(RutaInicial);
                    int newHeight = int.Parse(newBitmap.PhysicalDimension.Height.ToString());
                    int newWidth = int.Parse(newBitmap.PhysicalDimension.Width.ToString());

                    Bitmap newImage = new Bitmap(newWidth, newHeight);
                    using (Graphics gr = Graphics.FromImage(newImage))
                    {
                        gr.SmoothingMode = SmoothingMode.HighQuality;
                        gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        gr.DrawImage(imageIn, new Rectangle(0, 0, newWidth, newHeight));

                    }

                    myEncoderParameters = new EncoderParameters(1);
                    myEncoderParameter = new EncoderParameter(myEncoder, 4L);
                    myEncoderParameters.Param[0] = myEncoderParameter;
                    newImage.SetResolution(dpi, dpi);
                    newImage.Save(RutaFinal, myImageCodecInfo, myEncoderParameters);

                    newImage.Dispose();
                    imageIn.Dispose();
                    // bmPhoto.Dispose();
                    newBitmap.Dispose();
                }
                GC.Collect();
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
                GC.Collect();
            }
        }

        private void GuardarImagen(int dpi, string RutaOrigen, string RutaDestino)
        {


            Bitmap bmp1 = new Bitmap(RutaOrigen);
            ImageCodecInfo jgpEncoder = GetEncoder(ImageFormat.Jpeg);
            System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
            EncoderParameters myEncoderParameters = new EncoderParameters(1);
            EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, (long)EncoderValue.CompressionCCITT4);
            bmp1.SetResolution(dpi, dpi);
            myEncoderParameters.Param[0] = myEncoderParameter;
            bmp1.Save(RutaDestino, jgpEncoder, myEncoderParameters);
            GC.Collect();

        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {

            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    GC.Collect();
                    return codec;
                }
            }
            GC.Collect();
            return null;
        }

        private static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                {
                    GC.Collect();
                    return encoders[j];
                }
            }
           
            GC.Collect();
            return null;
        }

        private void EscribirArchivoPlanoCSV(DataTable resp)
        {
            try
            {
                Dictionary<string, object> parametros = new Dictionary<string, object>();
                parametros.Add("@Name", "RutaExport");
                parametros.Add("@opcion", "1");
                DataTable Ruta = ExecuteSPCon(conexion, "[dbo].[Get_Traking_Set]", parametros);

                string RutaExport = Ruta.Rows[0][0].ToString();
                if (!RutaExport.EndsWith("\\"))
                {
                    RutaExport = RutaExport + "\\";
                }
                string Nombre = RutaExport + resp.Rows[0]["NumeroCodBar"] + "_" + resp.Rows[0]["cupon"] + "_" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".CVS"; ;
                string Linea = "";

                foreach (DataRow Dato in resp.Rows)
                {

                    Linea = Dato["CoditoTipoDocumental"].ToString() + "|"
                        + Dato["NumeroCodBar"].ToString() + "|"
                        + Dato["NoIdent"].ToString() + "|"
                        + Dato["Nombre1"].ToString() + "|"
                        + Dato["Nombre2"].ToString() + "|"
                        + Dato["Apellido1"].ToString() + "|"
                        + Dato["Apellido2"].ToString() + "|"
                        + Dato["Direccion"].ToString() + "|"
                        + Dato["TelFijo"].ToString() + "|"
                        + Dato["TelMovil"].ToString() + "|"
                        + Dato["Email"].ToString() + "|"
                        + Dato["Braile"].ToString() + "|"
                        + Dato["Invidente"].ToString() + "|"
                        + Dato["NiveldeEstudio"].ToString() + "|"
                        + Dato["Genero"].ToString() + "|"
                        + Dato["FechaExpedicion"].ToString() + "|"
                        + Dato["FechaInscripcion"].ToString() + "|"
                        + Dato["Discapacidad"].ToString() + "|"
                        + Dato["cupon"].ToString() + "|"
                        + Dato["Novedades"].ToString() + "|"
                        + Dato["Fullpath"].ToString();

                }



                if (File.Exists(Nombre))
                {
                    StreamWriter objWr = File.AppendText(Nombre);

                    objWr.WriteLine(Linea);
                    objWr.Flush();
                    objWr.Close();
                }
                else
                {
                    StreamWriter objWr = File.CreateText(Nombre);

                    objWr.WriteLine(Linea);
                    objWr.Flush();
                    objWr.Close();
                }
                GC.Collect();
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
                GC.Collect();
            }
        }

        private void EscribirArchivoPlano(DataTable resp)
        {
            try
            {
                Dictionary<string, object> parametros = new Dictionary<string, object>();
                parametros.Add("@Name", "RutaExport");
                parametros.Add("@opcion", "1");
                DataTable Ruta = ExecuteSPCon(conexion, "[dbo].[Get_Traking_Set]", parametros);

                string RutaExport = Ruta.Rows[0][0].ToString();
                if (!RutaExport.EndsWith("\\"))
                {
                    RutaExport = RutaExport + "\\";
                }
                string Nombre = RutaExport + resp.Rows[0]["NumeroCodBar"] + "_" + resp.Rows[0]["cupon"] + "_" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".txt"; ;
                string Linea = "";

                foreach (DataRow Dato in resp.Rows)
                {

                    Linea = Dato["CoditoTipoDocumental"].ToString() + "|"
                        + Dato["NumeroCodBar"].ToString() + "|"
                        + Dato["NoIdent"].ToString() + "|"
                        + Dato["Nombre1"].ToString() + "|"
                        + Dato["Nombre2"].ToString() + "|"
                        + Dato["Apellido1"].ToString() + "|"
                        + Dato["Apellido2"].ToString() + "|"
                        + Dato["Direccion"].ToString() + "|"
                        + Dato["TelFijo"].ToString() + "|"
                        + Dato["TelMovil"].ToString() + "|"
                        + Dato["Email"].ToString() + "|"
                        + Dato["Braile"].ToString() + "|"
                        + Dato["Invidente"].ToString() + "|"
                        + Dato["NiveldeEstudio"].ToString() + "|"
                        + Dato["Genero"].ToString() + "|"
                        + Dato["FechaExpedicion"].ToString() + "|"
                        + Dato["FechaInscripcion"].ToString() + "|"
                        + Dato["Discapacidad"].ToString() + "|"
                        + Dato["cupon"].ToString() + "|"
                        + Dato["Novedades"].ToString() + "|"
                        + Dato["Fullpath"].ToString();

                }



                if (File.Exists(Nombre))
                {
                    StreamWriter objWr = File.AppendText(Nombre);

                    objWr.WriteLine(Linea);
                    objWr.Flush();
                    objWr.Close();
                }
                else
                {
                    StreamWriter objWr = File.CreateText(Nombre);

                    objWr.WriteLine(Linea);
                    objWr.Flush();
                    objWr.Close();
                }
                GC.Collect();
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
                GC.Collect();
            }
        }

        private string TreaerWSDL()
        {
            try
            {
                parametros = new Dictionary<string, object>();
                GC.Collect();
                return "http://172.27.244.32:8080/WSConsolidaMesa/WSCargarMesa?WSDL";

            }
            catch
            {
                GC.Collect();
                return null;
            }
        }

        public void ExcuteTrazabilidad(string nombre, DataTable tbl, int proceso)
        {
            Dictionary<string, object> parametros = new Dictionary<string, object>();
            parametros.Add("proceso", proceso);
            parametros.Add("nombre", nombre);
            parametros.Add("tabla", tbl);
            ExecuteSPCon(conexion, "[dbo].[Traking_Trazabilidad]", parametros);
            GC.Collect();
        }



#endregion

#region Metodos Basicos
        private DataTable ExecuteSP(string storeProcedure, Dictionary<string, object> parametro, bool isParametro = false)
        {
            SqlConnection conexion;

            if (!isParametro)
                conexion = GetConexion();
            else
                conexion = GetConexionParamerto();

            conexion.Open();
            SqlCommand comando = new SqlCommand(storeProcedure, conexion);
            foreach (KeyValuePair<string, object> aux in parametro)
                comando.Parameters.Add(new SqlParameter(aux.Key, aux.Value));
            comando.CommandType = CommandType.StoredProcedure;
            comando.CommandTimeout = 0;
            SqlDataReader objRead = comando.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(objRead);
            conexion.Close();
            conexion.Dispose();
            GC.Collect();
            return dt;
        }

        private DataTable ExecuteSPCon(SqlConnection conexion, string storeProcedure, Dictionary<string, object> parametro, bool isParametro = false)
        {

            SqlCommand comando = new SqlCommand(storeProcedure, conexion);
            foreach (KeyValuePair<string, object> aux in parametro)
                comando.Parameters.Add(new SqlParameter(aux.Key, aux.Value));
            comando.CommandType = CommandType.StoredProcedure;
            comando.CommandTimeout = 0;
            SqlDataReader objRead = comando.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(objRead);
            GC.Collect();
            return dt;
        }

        private DataTable ExecuteQuery(string query, Dictionary<string, object> parametro, bool isParametro = false)
        {
            SqlConnection conexion;

            //if (!isParametro)
            conexion = GetConexion();
            //else
            //    conexion = GetConexionParamerto();

            conexion.Open();

            SqlCommand comando = new SqlCommand(query, conexion);
            foreach (KeyValuePair<string, object> aux in parametro)
                comando.Parameters.Add(new SqlParameter(aux.Key, aux.Value));
            comando.CommandType = CommandType.Text;
            comando.CommandTimeout = 0;
            SqlDataReader objRead = comando.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(objRead);
            conexion.Close();
            conexion.Dispose();
            GC.Collect();
            return dt;
        }

        private string GetParametroIni(string key)
        {
            String ehGlobal = GetIniFileString("GlobalPath", "Directory", _Ehlocal);
            if (!ehGlobal.EndsWith("\\")) ehGlobal += "\\";
            ehGlobal += "ehGlobal.ini";

            string valor = GetIniFileString(key, "Parametros", ehGlobal);
            GC.Collect();
            return valor;
        }

        private SqlConnection GetConexion()
        {
            String ehGlobal = GetIniFileString("GlobalPath", "Directory", _Ehlocal);
            if (!ehGlobal.EndsWith("\\")) ehGlobal += "\\";
            ehGlobal += "ehGlobal.ini";

            string Server = GetIniFileString("Server", "Database", ehGlobal);
            string DatabaseName = GetIniFileString("DatabaseName", "Database", ehGlobal);
            string UserName = GetIniFileString("UserName", "Database", ehGlobal);
            string Password = GetIniFileString("Password", "Database", ehGlobal);

            SqlConnection sConnection = new SqlConnection();
            sConnection.ConnectionString = @"Data Source=" + Server + ";Initial Catalog=" + DatabaseName + ";User ID=" + UserName + ";Password=" + Password + ";Connect Timeout=60000;MultipleActiveResultSets=True";
            GC.Collect();
            return sConnection;
        }

        private SqlConnection GetConexionParamerto()
        {
            String ehGlobal = GetIniFileString("GlobalPath", "Directory", _Ehlocal);
            if (!ehGlobal.EndsWith("\\")) ehGlobal += "\\";
            ehGlobal += "ehGlobal.ini";

            string Server = GetIniFileString("Server", "Parametros", ehGlobal);
            string DatabaseName = GetIniFileString("DatabaseName", "Parametros", ehGlobal);
            string UserName = GetIniFileString("UserName", "Parametros", ehGlobal);
            string Password = GetIniFileString("Password", "Parametros", ehGlobal);

            SqlConnection sConnection = new SqlConnection();
            sConnection.ConnectionString = @"Data Source=" + Server + ";Initial Catalog=" + DatabaseName + ";User ID=" + UserName + ";Password=" + Password + ";Connect Timeout=60000;MultipleActiveResultSets=True";
            GC.Collect();
            return sConnection;
        }

        public static string GetIniFileString(string key, string category, string iniFile)
        {
            string defaultValue = string.Empty;
            string returnString = new string(' ', 1024);
            GetPrivateProfileString(category, key, defaultValue, returnString, 1024, iniFile);
            GC.Collect();
            return returnString.Split('\0')[0];
        }

        public void Log(string log)
        {
            StreamWriter escribir = File.CreateText(@"" + "Z:\\Compartido\\Log\\log_" + DateTime.Now.ToString("yyyyMMddh") + ".txt");

            escribir.WriteLine(log);
            escribir.Close();
            GC.Collect();

        }
#endregion

    }
}