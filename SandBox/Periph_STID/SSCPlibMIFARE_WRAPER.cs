
/** 
 
  STid - electronic identification - RFId
  www.stid.com
  
  SSCP  Mifare Global Library Wrapping Unit
  Wrapping unit for SSCPlibMifareGlobal.dll functions
 
  See MANU_SSCPlibMifareGlobal.pdf document for explanations and how to use library

  version | author |  date   | comments
  --------|--------|---------|------------------------
  1.0     | éµ     | 06/2009 | initial version
  1.0.1   | éµ     | 10/2009 | add (CharSet = CharSet.Unicode) in DllImport for now Unicode string
  1.1.0	  | éµ	   | 04/2011 | regrouping DESFire,Plus,ULight lib together in ONE. No more Classic lib, use M Plus Level1
                             | new reader's functions and  Diversifications


**/


using System;
using System.Runtime.InteropServices;



namespace SandBox.Periph_STID
{
    class SSCPlibMIFARE
    {

           // Communication crypto mode
        public const byte m_plain = 0 ;  // Plain communication
        public const byte m_sig   = 1 ;  // Authenticated communication
        public const byte m_enc   = 2 ;  // Enciphered commuinication
        public const byte m_sigenc= 3 ;  // Authenticated AND enciphered communication

        // Serial communication baudrate
        public const byte br_9600   = 0;
        public const byte br_19200  = 1;
        public const byte br_38400  = 2;
        public const byte br_57600  = 3;
        public const byte br_115200 = 4;

        // Communication type
        public const byte ct_rs232 = 0; // RS232 serial communication
        public const byte ct_rs485 = 1; // RS485 addressable serial communication
        public const byte ct_usb   = 2; // Native USB not yet Reader implemented, you sould use ct_rs232 with USB converter provided
        public const byte ct_tcp   = 3; // TCP reader not yet managed by the Lib
        public const byte ct_udp   = 4; // UDP reader not yet managed by the Lib
        
        
        // Library errors
        public const UInt16 SSCPLIB_ERROR_MUST_INITIALIZE     = 0xFF01; // Library NOT initialized
        public const UInt16 SSCPLIB_ERROR_ALREADY_INITIALIZED = 0xFF02; // Library is already initialized
        public const UInt16 SSCPLIB_ERROR_BAD_COM_PORT        = 0xFF03; // COMPORT selected is already openned by another software, or is unreachable
        public const UInt16 SSCPLIB_ERROR_BAD_PARAMETER       = 0xFF04; // Bad parameter (for ex. bad name for COMPORT)
        public const UInt16 SSCPLIB_ERROR_EXCEPTION           = 0xFF05; // Library has launched an exception (for ex. COMPORT is no more accessible or not connected, no more free RAM, ...)

        
        // CMD/Error types
        public const byte SSCP_TYPE_READER       = 0x00; // Reader command
        public const byte SSCP_TYPE_MIFARE       = 0x01; // Mifare Standard command (UNused by this Lib)
        public const byte SSCP_TYPE_DESFIRE      = 0x02; // Mifare DESFire command  AND Mifare DESFIRE EVolution 1
        public const byte SSCP_TYPE_MIFARE_PLUS  = 0x03; // Mifare Plus command
        public const byte SSCP_TYPE_MIFARE_ULIGHT= 0x05; // Mifare Ultra Light
        public const byte SSCP_TYPE_PN532        = 0x06; // PN532 type
        public const byte SSCP_TYPE_CPS3         = 0x09; // CPS3

        
        public const UInt16 SSCP_OK = 0x00; // operation ok

        
        // to have errors' description you should use SSCP_GetErrorMsg

        public const int SSCP_READER_AUTH_ERR             = 0x01 ; // Authentication error
        public const int SSCP_READER_BAD_PARAM_DATA_ERR   = 0x02 ; // Bad parameter data , or IF RFU is not 0 (case of incorrect DeCipher), or EncDec crypto error
        public const int SSCP_READER_CRC_ERR              = 0x03 ; // CRC Frame error
        public const int SSCP_READER_REC_DATA_LEN_ERR     = 0x04 ; // Frame received length error, not length expected (shorter!)
        public const int SSCP_READER_SIGN_ERR             = 0x05 ; // In Authentication mode (sign) Frame received contain Bad Signature
        public const int SSCP_READER_COM_TIMEOUT          = 0x06 ; // Response too late or no response
        public const int SSCP_READER_BAD_COMMAND_CODE     = 0x07 ; //
        public const int SSCP_READER_BAD_COMMAND_TYPE     = 0x08 ; //
        public const int SSCP_READER_UPDATE_FIRMWARE_ERROR= 0x09 ;
        public const int SSCP_READER_BAD_CMD_ACK          = 0x0A ; // BAD CMD ACK, ACK received different from CMD sent   +
        public const int SSCP_READER_COMM_MODE_NOT_ALLOWED= 0x0B ;


        public const int SSCP_DESFIRE_NO_CHANGES                  = 0x0C ;
        public const int SSCP_DESFIRE_OUT_OF_EEPROM_ERROR         = 0x0E ;
        public const int SSCP_DESFIRE_ILLEGAL_COMMAND_CODE        = 0x1C ;
        public const int SSCP_DESFIRE_INTEGRITY_ERROR             = 0x1E ;
        public const int SSCP_DESFIRE_NO_SUCH_KEY                 = 0x40 ;
        public const int SSCP_DESFIRE_LENGTH_ERROR                = 0x7E ;
        public const int SSCP_DESFIRE_PERMISSION_DENIED           = 0x9D ;
        public const int SSCP_DESFIRE_BAD_PARAM_DATA_ERR          = 0x9E;
        public const int SSCP_DESFIRE_APPLICATION_NOT_FOUND       = 0xA0 ;
        public const int SSCP_DESFIRE_APPL_INTEGRITY_ERROR        = 0xA1 ;
        public const int SSCP_DESFIRE_AUTHENTICATION_ERROR        = 0xAE ;
        public const int SSCP_DESFIRE_ADDITIONAL_FRAME            = 0xAF ;
        public const int SSCP_DESFIRE_BOUNDARY_ERROR              = 0xBE ;
        public const int SSCP_DESFIRE_PICC_INTEGRITY_ERROR        = 0xC1 ;
        public const int SSCP_DESFIRE_COMMAND_ABORTED             = 0xCA ;
        public const int SSCP_DESFIRE_PICC_DISABLED_ERROR         = 0xCD ;
        public const int SSCP_DESFIRE_COUNT_ERROR                 = 0xCE ;
        public const int SSCP_DESFIRE_DUPLICATE_ERROR             = 0xDE ;
        public const int SSCP_DESFIRE_EEPROM_ERROR                = 0xEE ;
        public const int SSCP_DESFIRE_FILE_NOT_FOUND              = 0xF0 ;
        public const int SSCP_DESFIRE_FILE_INTEGRITY_ERROR        = 0xF1 ;

        // STid error codes for DESFire
        public const int SSCP_DESFIRE_TWO_TAGS_ERR = 0x01; // More than ONE tag 
        public const int SSCP_DESFIRE_BAD_TAG_TYPE_ERR = 0x02; // Not an DESFire Tag

        // Reader - PN532 (HF) - MIFARE - errors
        public const int SSCP_PN532_MIFAREPLUS_DATA_FORMAT_ERROR = 0x13;
        public const int SSCP_PN532_MIFAREPLUS_AUTHENTICATION_ERROR = 0x14;
        public const int SSCP_PN532_MIFAREPLUS_COMMAND_NOT_ACCEPTABLE = 0x27;

        // Error code for MIFARE Plus
        public const int SSCP_MIFAREPLUS_WRONG_CRC_VALUE_1ST_EXCHANGED = 0x05;
        public const int SSCP_MIFAREPLUS_WRONG_CRC_VALUE_2ND_EXCHANGED = 0x01;
        public const int SSCP_MIFAREPLUS_ACK_CONDITIONS_OF_USE_NOT_SATISFIED = 0x04;

        public const int SSCP_MIFAREPLUS_AUTHENTICATE_ERROR = 0x06;
        public const int SSCP_MIFAREPLUS_COMMAND_OVERFLOW = 0x07;
        public const int SSCP_MIFAREPLUS_INVALID_MAC = 0x08;
        public const int SSCP_MIFAREPLUS_INVALID_BLOCK_NUMBER = 0x09;
        public const int SSCP_MIFAREPLUS_NOT_EXISTING_BLOCK_NUMBER = 0x0A;
        public const int SSCP_MIFAREPLUS_ER_CONDITIONS_OF_USE_NOT_SATISFIED = 0x0B;
        public const int SSCP_MIFAREPLUS_LENGTH_ERROR = 0x0C;
        public const int SSCP_MIFAREPLUS_GENERAL_MANIPULATION_ERROR = 0x0F;
        public const int SSCP_MIFAREPLUS_TWO_TAGS_ERROR = 0xF0;
        public const int SSCP_MIFAREPLUS_BAD_TAG_TYPE = 0xF1;
        public const int SSCP_MIFAREPLUS_PARAMETER_ERROR = 0xF2;

        
        /////// MIFARE Ultralight C error codes

        public const int SSCP_ULTRALIGHT_ERR_TWO_TAGS = 0x02;
        public const int SSCP_ULTRALIGHT_ERR_BAD_TAG_TYPE = 0x03;
        public const int SSCP_ULTRALIGHT_ERR_PARAMETER = 0x06;
        public const int SSCP_ULTRALIGHT_ERR_AUTHENTICATE = 0x07;

        public const int SSCP_PN532_ULTRALIGHT_TIME_OUT = 0x01;
        public const int SSCP_PN532_ULTRALIGHT_FRAMING_ERROR = 0x05;
        public const int SSCP_PN532_ULTRALIGHT_DATA_FORMAT_ERROR = 0x13;
        public const int SSCP_PN532_ULTRALIGHT_AUTHENTICATION_ERROR = 0x14;
        public const int SSCP_PN532_ULTRALIGHT_COMMAND_NOT_ACCEPTABLE = 0x27;
        public const int SSCP_PN532_ULTRALIGHT_RFID_NO_DATA_EXCHANGE = 0xF1;


        /////// STid CPS3 error codes
        public const byte SSCP_CPS3_ERROR_GENERIC  	= 0x01;
        public const byte SSCP_CPS3_PARAMETER_ERROR   = 0x9E ;

        /////// native CPS3 error codes
        // to have native CPS3 errors' description you should use SSCPc_GetErrorMsg

        public const int CPS3_WARNING_62XX_NO_INFO                 = 0x6200;
        public const int CPS3_WARNING_RET_DAT_MAYBE_CORRUPTED      = 0x6281;
	    public const int CPS3_WARNING_EOF_REACHED_BEFORE_READALL   = 0x6282;
        public const int CPS3_WARNING_SELECTEDFILE_DEACTIVATED     = 0x6283;
        public const int CPS3_WARNING_FCI_NOT_CORRECTLY_FORMATTED  = 0x6284;
        public const int CPS3_WARNING_SELECTEDFILE_TERMINATED      = 0x6285;
        public const int CPS3_WARNING_NO_INPUTDAT_AVAILABLE        = 0x6286;
        public const int CPS3_WARNING_63XX_NO_INFO                 = 0x6300;
        public const int CPS3_WARNING_FILE_ALREADY_FILLED_UP       = 0x6381;
        public const int CPS3_ERROR_EXECUTION                      = 0x6400;
	    public const int CPS3_ERROR_RESPONSE_REQUIRED              = 0x6401;
        public const int CPS3_ERROR_65XX_NO_INFO                   = 0x6500;
	    public const int CPS3_ERROR_MEMORY_FAILURE                 = 0x6581;
        public const int CPS3_ERROR_BAD_LENGTH                     = 0x6700;
        public const int CPS3_ERROR_68XX_NO_INFO                   = 0x6800;
        public const int CPS3_ERROR_LOG_CHANNEL_NOT_SUPPORTED      = 0x6881;
        public const int CPS3_ERROR_SEC_MSG_NOT_SUPPORTED          = 0x6882;
        public const int CPS3_ERROR_LAST_CMD_EXPECTED              = 0x6883;
        public const int CPS3_ERROR_CMD_CHAINING_NOT_SUPPORTED     = 0x6884;
        public const int CPS3_ERROR_69XX_NO_INFO                   = 0x6900;
        public const int CPS3_ERROR_CMD_INCOMPATIBLE_FILE_STRUCT   = 0x6981;
        public const int CPS3_ERROR_SECURITY_STATUS_NOT_SATISFIED  = 0x6982;
        public const int CPS3_ERROR_AUTHENTICATION_METHOD_BLOCKED  = 0x6983;
        public const int CPS3_ERROR_REFERENCE_DATA_NOT_USABLE      = 0x6984;
        public const int CPS3_ERROR_CONDITION_OF_USE_NOT_SATISFIED = 0x6985;
        public const int CPS3_ERROR_CMD_NOT_ALLOWED                = 0x6986;
        public const int CPS3_ERROR_EXPECTED_SEC_MSG_DATOBJ_MISS   = 0x6987;
        public const int CPS3_ERROR_INCORRECT_SEC_MSG_DATOBJ       = 0x6988;
        public const int CPS3_ERROR_6AXX_NO_INFO                   = 0x6A00;
        public const int CPS3_ERROR_INCORRECT_PARAMS               = 0x6A80;
        public const int CPS3_ERROR_FUNCTION_NOT_SUPPORTED         = 0x6A81;
        public const int CPS3_ERROR_FILE_APP_NOT_FOUND             = 0x6A82;
 	    public const int CPS3_ERROR_RECORD_NOT_FOUND               = 0x6A83;
 	    public const int CPS3_ERROR_NOT_ENOUGH_MEMORY_IN_FILE      = 0x6A84;
        public const int CPS3_ERROR_INCORRECT_PARAMS_P1P2          = 0x6A86;
 	    public const int CPS3_ERROR_Lc_INCONSISTENT_P1P2           = 0x6A87;
        public const int CPS3_ERROR_REF_DATA_NOT_FOUND             = 0x6A88;
 	    public const int CPS3_ERROR_FILE_ALREADY_EXISTS            = 0x6A89;
        public const int CPS3_ERROR_DF_NAME_ALREADY_EXISTS         = 0x6A8A;

       
      // Path to library
      public const string LIB_PATH_NAME = ".\\Resources\\STID\\SSCPlibMifareGlobal.dll";

      #region Library generic functions


        // The FIRST command to invoke, initializes library !
        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCP_Initialize")]
        public static extern UInt16 SSCP_Initialize();

        //The LAST command, frees the library
        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCP_Terminate")]
        public static extern UInt16 SSCP_Terminate();

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCP_GetCOMType")]
        public static extern UInt16 SSCP_GetCOMType(ref byte comtype);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCP_SetCOMType")]
        public static extern UInt16 SSCP_SetCOMType(byte comtype);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCP_SetAutoConnect")]
        public static extern UInt16 SSCP_SetAutoConnect(byte autoconnect);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCP_GetAutoConnect")]
        public static extern UInt16 SSCP_GetAutoConnect(ref byte autoconnect);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCP_SetMode")]
        public static extern UInt16 SSCP_SetMode(byte mode);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCP_GetMode")]
        public static extern UInt16 SSCP_GetMode(ref byte mode);

        [DllImport(LIB_PATH_NAME, CharSet = CharSet.Unicode, EntryPoint = "SSCP_Serial_SetPort")]
        public static extern UInt16 SSCP_Serial_SetPort(string COMPort);

        [DllImport(LIB_PATH_NAME, CharSet = CharSet.Unicode, EntryPoint = "SSCP_Serial_GetPort")]
        public static extern UInt16 SSCP_Serial_GetPort(ref string COMPort);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCP_Serial_SetBaudRate")]
        public static extern UInt16 SSCP_Serial_SetBaudRate(byte baudrate);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCP_Serial_GetBaudRate")]
        public static extern UInt16 SSCP_Serial_GetBaudRate(ref byte baudrate);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCP_Serial_SetTimeout")]
        public static extern UInt16 SSCP_Serial_SetTimeout(int readconstant,
                                                           int readmultiplier,
                                                           int writeconstant,
                                                           int writemultiplier);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCP_Serial_GetTimeout")]
        public static extern UInt16 SSCP_Serial_GetTimeout(ref int readconstant,
                                                           ref int readmultiplier,
                                                           ref int writeconstant,
                                                           ref int writemultiplier);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCP_Connect")]
        public static extern UInt16 SSCP_Connect();

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCP_Disconnect")]
        public static extern UInt16 SSCP_Disconnect();

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCP_Serial_Get485Address")]
        public static extern UInt16 SSCP_Serial_Get485Address(ref int address);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCP_Serial_Set485Address")]
        public static extern UInt16 SSCP_Serial_Set485Address(int address);


        /// ERROR message function
        [DllImport(LIB_PATH_NAME, CharSet = CharSet.Unicode, EntryPoint = "SSCP_GetErrorMsg")]
        public static extern UInt16 SSCP_GetErrorMsg(UInt16 LID,      // Language ID
                                                     UInt16 Error,
                                                     ref string ErrorMsg);
      #endregion

      #region Crypto functions
        // No communication with reader

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCP_Diversify3DES")]
        public static extern UInt16 SSCP_Diversify3DES(byte blocknb,
                                                       [In] byte[] currentKey,
                                                       [In] byte[] UID,
                                                       [In] byte[] diversificationKey,
                                                       [Out] byte[] diversifiedKey );

        //////////V1.1
        [DllImport(LIB_PATH_NAME, CharSet = CharSet.Unicode, EntryPoint = "SSCP_RSA_GenerateKeyPair")]
        public static extern UInt16 SSCP_RSA_GenerateKeyPair(UInt16 keySize,
                                                             string pubKey,
                                                             string privKey);

        [DllImport(LIB_PATH_NAME, CharSet = CharSet.Unicode, EntryPoint = "SSCP_RSA_LoadKeys")]
        public static extern UInt16 SSCP_RSA_LoadKeys(string pubKey,
                                                      string privKey);

        [DllImport(LIB_PATH_NAME, CharSet = CharSet.Unicode, EntryPoint = "SSCP_RSA_Sign_PKCS_V15")]
        public static extern UInt16 SSCP_RSA_Sign_PKCS_V15(string msg,
                                                           byte hashType,
                                                           string Sign);

        [DllImport(LIB_PATH_NAME, CharSet = CharSet.Unicode, EntryPoint = "SSCP_RSA_Verify_PKCS_V15")]
        public static extern UInt16 SSCP_RSA_Verify_PKCS_V15(string msg,
                                                             string sign,
                                                             byte hashType,
                                                             ref Boolean Verified);

        [DllImport(LIB_PATH_NAME, CharSet = CharSet.Unicode, EntryPoint = "SSCP_RSA_Encrypt")]
        public static extern UInt16 SSCP_RSA_Encrypt(string msg,
                                                     Boolean b_256,
                                                     string Enc);

        [DllImport(LIB_PATH_NAME, CharSet = CharSet.Unicode, EntryPoint = "SSCP_RSA_Decrypt")]
        public static extern UInt16 SSCP_RSA_Decrypt(string Enc,
                                                     Boolean b_256,
                                                     string Dec);
        //////////V1.1

      #endregion
        
      #region Reader functions
  

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPr_Authenticate")]
        public static extern UInt16 SSCPr_Authenticate(byte mode);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPr_ResetAuthenticate")]
        public static extern UInt16 SSCPr_ResetAuthenticate();

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPr_ChangeReaderKeys")]
        public static extern UInt16 SSCPr_ChangeReaderKeys(byte onlySoftKeys,
                                                           byte keymode,
                                                           [In] byte[] SignKey,
                                                           [In] byte[] CipherKey );

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPr_Scan_14443_A")]
        public static extern UInt16 SSCPr_Scan_14443_A(ref byte nbCard,
                                                       ref byte CardType,
                                                       ref byte CardSize,
                                                       ref byte Info,
                                                       ref byte UIDLen,
                                                       [Out] byte[] UID,
                                                       [Out] byte[] ATS );

 
        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPr_Scan_14443_3B")]
        public static extern UInt16 SSCPr_Scan_14443_3B(ref byte nbCard,
                                                        ref byte UIDLen,
                                                        [Out] byte[] UID);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPr_OutPuts")]
        public static extern UInt16 SSCPr_OutPuts(byte LEDcolor,
                                                       byte LEDduration,
                                                       byte BuzzerDuration);

        [DllImport(LIB_PATH_NAME, CharSet = CharSet.Unicode, EntryPoint = "SSCPr_GetInfos")]
        public static extern UInt16 SSCPr_GetInfos(byte autoport,
                                                   byte autobaud,
                                                   ref byte baudrate,
                                                   ref byte addr485,
                                                   ref byte version,
                                                   ref UInt16 volt,
                                                   ref string infoports);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPr_SetBaudRate")]
        public static extern UInt16 SSCPr_SetBaudRate(byte Baudrate);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPr_Set485Address")]
        public static extern UInt16 SSCPr_Set485Address(byte address);
        
        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPr_SetAllowedCommModes")]
        public static extern UInt16 SSCPr_SetAllowedCommModes(byte plain,          // =1 allow plain comm
                                                                   byte sign,           // =1 allow plain authenticated comm
                                                                   byte enc,            // =1 allow plain enciphered comm
                                                                   byte sign_enc);      // =1 allow plain authenticated AND enciphered comm; Frozen to 1 by lib

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPr_SetTamperSwitchSettings")]
        public static extern UInt16 SSCPr_SetTamperSwitchSettings(byte activate,
                                                                       byte resettodefault,
                                                                       byte eraseRFIDkeys);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPr_GetTamperSwitchInfos")]
        public static extern UInt16 SSCPr_GetTamperSwitchInfos(ref byte activate,
                                                                    ref byte resettodefault,
                                                                    ref byte eraseRFIDkeys,
                                                                    ref byte currentstate);
        
        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPr_LoadSKB")]
        public static extern UInt16 SSCPr_LoadSKB();


        /////////V1.1
        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPr_Scan_A_Raw")]
        public static extern UInt16 SSCPr_Scan_A_Raw(byte RATS,
                                                     ref byte nbCard,
                                                     ref UInt16 ATQA,
                                                     ref byte SAK,
                                                     ref byte UIDLen,
                                                     [Out] byte[] UID,
                                                     [Out] byte[] ATS);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPr_Autonomous_Stop")]
        public static extern UInt16 SSCPr_Autonomous_Stop();

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPr_Autonomous_Start")]
        public static extern UInt16 SSCPr_Autonomous_Start();

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPr_Autonomous_Output")]
        public static extern UInt16 SSCPr_Autonomous_Output(byte OutConf,
                                                            byte Conf,
                                                            byte Len);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPr_Autonomous_BuzzLed")]
        public static extern UInt16 SSCPr_Autonomous_BuzzLed(byte defAction,
                                                             byte defBadge,
                                                             byte LEDDuration,
                                                             byte BuzzDuration);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPr_ConfAuthenticate")]
        public static extern UInt16 SSCPr_ConfAuthenticate(byte Mode,
                                                           UInt16 Value);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPr_Transceive")]
        public static extern UInt16 SSCPr_Transceive(UInt16 lenDataIn,
                                                     [In] byte[] DataIn,
                                                     ref UInt16 lenDataOut,
                                                     [Out] byte[] DataOut );

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPr_RSA_LoadPublicKey")]
        public static extern UInt16 SSCPr_RSA_LoadPublicKey(byte keySize,
                                                            [In] byte[] pubKey );
        
        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPr_RSA_PKCSVerify")]
        public static extern UInt16 SSCPr_RSA_PKCSVerify(byte hashType,
                                                         UInt16 msgLen,
                                                         [In] byte[] msg,
                                                         byte sigLen,
                                                         [In] byte[] sig );
        //////////V1.1

      #endregion
        
      #region MIFARE® DESFire functions

       
        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPd_Scan")]
        public static extern UInt16 SSCPd_Scan(ref byte nbCard,
                                               ref byte UIDLen,
                                               [Out] byte[] UID,
                                               [Out] byte[] ATS);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPd_Release")]
        public static extern UInt16 SSCPd_Release();

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPd_Format")]
        public static extern UInt16 SSCPd_Format();

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPd_GetVersion")]
        public static extern UInt16 SSCPd_GetVersion([Out] byte[] Versions);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPd_GetCardUID")]
        public static extern UInt16 SSCPd_GetCardUID([Out] byte[] UID);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPd_FreeMem")]
        public static extern UInt16 SSCPd_FreeMem(ref int FreeMem);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPd_SetConfiguration")]
        public static extern UInt16 SSCPd_SetConfiguration(byte Option,
                                                                 byte DataLen,
                                                                 [In] byte[] Data);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPd_PPS")]
        public static extern UInt16 SSCPd_PPS(byte BRIT,byte BRTI);

        
        /// Security functions

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPd_Authenticate")]
        public static extern UInt16 SSCPd_Authenticate( byte KeyLocation,
                                                        byte KeyNb,
                                                        byte CryptoMethod,
                                                        byte Index);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPd_ChangeKeySettings")]
        public static extern UInt16 SSCPd_ChangeKeySettings( byte ChangeKeyAccessRights,
                                                             byte ConfChgbl,
                                                             byte FreeCD,
                                                             byte FreeDir,
                                                             byte AllowMKchange); 

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPd_GetKeySettings")]
        public static extern UInt16 SSCPd_GetKeySettings( ref byte ChangeKeyAccessRights,
                                                          ref byte ConfChgbl,
                                                          ref byte FreeCD,
                                                          ref byte FreeDir,
                                                          ref byte AllowMKchange,
                                                          ref byte NbKeys,
                                                          ref byte CryptoMethod); 
        
        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPd_GetKeyVersion")]
        public static extern UInt16 SSCPd_GetKeyVersion( byte KeyNb,
                                                         ref byte KeyVersion); 

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPd_ChangeKey")]
        public static extern UInt16 SSCPd_ChangeKey( byte KeyNumber,
                                                     byte CryptoMethod,
                                                     byte AESKeyVersion,
                                                     [Out] byte[] newKey,  // 16 bytes 3DES key
                                                     [Out] byte[] oldKey); // 16 bytes 3DES key
  
        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPd_LoadKey")]
        public static extern UInt16 SSCPd_LoadKey( byte Save,
                                                   [Out] byte[] Key,  // 16 bytes 3DES key
                                                    byte Index);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPd_ChangeKeyIndex")]
        public static extern UInt16 SSCPd_ChangeKeyIndex( byte KeyNumber,
                                                          byte CryptoMethod,
                                                          byte AESKeyVersion,
                                                          byte NewKeyIndex,
                                                          byte OldKeyIndex,
                                                          byte RFU); 


        /// Application functions

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPd_CreateApplication")]
        public static extern UInt16 SSCPd_CreateApplication( UInt32 AID,
                                                             byte ChangeKeyAccess,
                                                             byte ConfChgbl,
                                                             byte FreeCD,
                                                             byte FreeDir,
                                                             byte AllowMKchange,
                                                             byte NbMaxKey, 
                                                             byte CryptoMethod);
        
        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPd_GetApplications")]
        public static extern UInt16 SSCPd_GetApplications( ref byte nbApps,
                                                           [Out] byte[] AIDs); 

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPd_SelectApplication")]
        public static extern UInt16 SSCPd_SelectApplication( UInt32 AID);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPd_DeleteApplication")]
        public static extern UInt16 SSCPd_DeleteApplication( UInt32 AID);


        /// File functions
        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPd_GetFileIDs")]
        public static extern UInt16 SSCPd_GetFileIDs( ref byte NbFileIDs,
                                                      [Out] byte[] FileIDs);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPd_CreateStdDataFile")]
        public static extern UInt16 SSCPd_CreateStdDataFile( byte FileNb,
                                                             byte ComSet,
                                                             byte ReadAcKeyNb,
                                                             byte WriteAcKeyNb,
                                                             byte ReadWriteAcKeyNb,
                                                             byte ChgAcRightKeyNb,
                                                             UInt32 FileSize);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPd_CreateBackupDataFile")]
        public static extern UInt16 SSCPd_CreateBackupDataFile( byte FileNb,
                                                                byte ComSet,
                                                                byte ReadAcKeyNb,
                                                                byte WriteAcKeyNb,
                                                                byte ReadWriteAcKeyNb,
                                                                byte ChgAcRightKeyNb,
                                                                UInt32 FileSize);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPd_GetFileSettings")]
        public static extern UInt16 SSCPd_GetFileSettings( byte FileID,
                                                           ref byte FileType,
                                                           ref byte CommMode,
                                                           ref UInt16 AccessRights,
                                                           ref int info1,
                                                           ref int info2,
                                                           ref int info3,
                                                           ref int info4);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPd_ChangeFileSettings")]
        public static extern UInt16 SSCPd_ChangeFileSettings( byte FileID,
                                                              byte CommandMode,
                                                              byte FileCommMode,
                                                              byte ReadAcKeyNb,
                                                              byte WriteAcKeyNb,
                                                              byte ReadWriteAcKeyNb,
                                                              byte ChgAcRightKeyNb);
        
        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPd_DeleteFile")]
        public static extern UInt16 SSCPd_DeleteFile( byte FileID);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPd_CreateValueFile")]
        public static extern UInt16 SSCPd_CreateValueFile( byte FileNb,
                                                           byte ComSet,
                                                           byte ReadAcKeyNb,
                                                           byte WriteAcKeyNb,
                                                           byte ReadWriteAcKeyNb,
                                                           byte ChgAcRightKeyNb,
                                                           byte limitedcredit,
                                                           int initialvalue,
                                                           int lowerlimit,
                                                           int upperlimit);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPd_CreateLinearRecordFile")]
        public static extern UInt16 SSCPd_CreateLinearRecordFile(byte FileNb,
                                                                 byte ComSet,
                                                                 byte ReadAcKeyNb,
                                                                 byte WriteAcKeyNb,
                                                                 byte ReadWriteAcKeyNb,
                                                                 byte ChgAcRightKeyNb,
                                                                 UInt32 RecordSize,
                                                                 UInt32 MaxNumOfRecord);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPd_CreateCyclicRecordFile")]
        public static extern UInt16 SSCPd_CreateCyclicRecordFile(byte FileNb,
                                                                 byte ComSet,
                                                                 byte ReadAcKeyNb,
                                                                 byte WriteAcKeyNb,
                                                                 byte ReadWriteAcKeyNb,
                                                                 byte ChgAcRightKeyNb,
                                                                 UInt32 RecordSize,
                                                                 UInt32 MaxNumOfRecord);


  // Data functions

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPd_WriteData")]
        public static extern UInt16 SSCPd_WriteData(byte CommMode,
                                                    byte FileID,
                                                    UInt32 offset,
                                                    UInt32 len,
                                                    [In] byte[] data);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPd_ReadData")]
        public static extern UInt16 SSCPd_ReadData(byte CommMode,
                                                   byte FileID,
                                                   UInt32 offset,
                                                   UInt32 len,
                                                   [Out] byte[] data);
 
        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPd_WriteRecord")]
        public static extern UInt16 SSCPd_WriteRecord(byte CommMode,
                                                      byte FileID,
                                                      UInt32 offset,
                                                      UInt32 len,
                                                      [In] byte[] data);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPd_ReadRecords")]
        public static extern UInt16 SSCPd_ReadRecords(byte CommMode,
                                                      byte FileID,
                                                      UInt32 offset,
                                                      UInt32 NbOfRecords,
                                                      ref byte dataLen,
                                                      [Out] byte[] data);


        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPd_ClearRecordFile")]
        public static extern UInt16 SSCPd_ClearRecordFile(byte FileID);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPd_CommitTransaction")]
        public static extern UInt16 SSCPd_CommitTransaction();

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPd_AbortTransaction")]
        public static extern UInt16 SSCPd_AbortTransaction();

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPd_GetValue")]
        public static extern UInt16 SSCPd_GetValue(byte CommMode,
                                                   byte FileID,
                                                   ref int value);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPd_Credit")]
        public static extern UInt16 SSCPd_Credit(byte CommMode,
                                                 byte FileID,
                                                 int value);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPd_Debit")]
        public static extern UInt16 SSCPd_Debit(byte CommMode,
                                                byte FileID,
                                                int value);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPd_LimitedCredit")]
        public static extern UInt16 SSCPd_LimitedCredit(byte CommMode,
                                                        byte FileID,
                                                        int value);

      #endregion

      #region MIFARE® Plus functions

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPp_Scan")]
        public static extern UInt16 SSCPp_Scan(ref byte nbCard,
                                               ref byte Info,
                                               ref byte UIDLen,
                                               [Out] byte[] UID,
                                               [Out] byte[] ATS);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPp_Release")]
        public static extern UInt16 SSCPp_Release();


        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPp_WritePerso")]
        public static extern UInt16 SSCPp_WritePerso(UInt16 BNR,
                                                     [In] byte[] Value);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPp_CommitPerso")]
        public static extern UInt16 SSCPp_CommitPerso();

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPp_AuthenticateSwitchLevel")]
        public static extern UInt16 SSCPp_AuthenticateSwitchLevel(byte toLevel,
                                                                  [In] byte[] AESkey);



        /// Security Level 1 - Mifare Classic compatible mode

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPp_LoadKey")]
        public static extern UInt16 SSCPp_LoadKey(byte AuthenticateMode,
                                                  byte KeyType,
                                                  byte SaveKey,
                                                  byte SectorNb,
                                                  [In] byte[] crypto1Key,
                                                  byte DiversifyKeys,
                                                  [In] byte[] AESKey);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPp_Diversify3DES")]
        public static extern UInt16 SSCPp_Diversify3DES(byte blocknb,
                                                        [In]  byte[] crypto1Key,       // 6  bytes = crypto1 key, mifare classic key lenght 
                                                        [In]  byte[] UID,              // 4  bytes = Mifare Classic UID len   
                                                        [In]  byte[] diversifyKey,     // 16 bytes = diversification key len 
                                                        [Out] byte[] diversifiedKey); // 6  bytes = crypto1 key diversified, mifare classic key lenght  

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPp_KeyDiversification")]
        public static extern UInt16 SSCPp_KeyDiversification([Out] byte[] diversifyKey); // 16  bytes, diversification key  

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPp_ReadBlock")]
        public static extern UInt16 SSCPp_ReadBlock(byte KeyType,
                                                    byte blocknb,
                                                    [Out] byte[] Block); // 16  bytes data block  

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPp_WriteBlock")]
        public static extern UInt16 SSCPp_WriteBlock(byte KeyType,
                                                     byte blocknb,
                                                     [In] byte[] Block); // 16  bytes data block

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPp_ReadSector")]
        public static extern UInt16 SSCPp_ReadSector(byte KeyType,
                                                     byte SectorNb,
                                                     byte Index,
                                                     [Out] byte[] Blocks); // 16*3=48  bytes data block  

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPp_WriteSector")]
        public static extern UInt16 SSCPp_WriteSector(byte KeyType,
                                                      byte SectorNb,
                                                      byte Index,
                                                      [In] byte[] Blocks); // 16*3=48  bytes data block  


        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPp_ReadBlockMacro")]
        public static extern UInt16 SSCPp_ReadBlockMacro(byte KeyType,
                                                         byte blocknb,
                                                         [Out] byte[] Block); // 16  bytes data block  

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPp_WriteBlockMacro")]
        public static extern UInt16 SSCPp_WriteBlockMacro(byte KeyType,
                                                          byte blocknb,
                                                          [In] byte[] Block); // 16  bytes data block

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPp_ReadSectorMacro")]
        public static extern UInt16 SSCPp_ReadSectorMacro(byte KeyType,
                                                          byte SectorNb,
                                                          byte Index,
                                                          [Out] byte[] Blocks); // 16*3=48  bytes data block  

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPp_WriteSectorMacro")]
        public static extern UInt16 SSCPp_WriteSectorMacro(byte KeyType,
                                                           byte SectorNb,
                                                           byte Index,
                                                           [In] byte[] Blocks); // 16*3=48  bytes data block  

        // Compatible Mifare Classic STid Indexed commands

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPp_LoadKeyBCE")]
        public static extern UInt16 SSCPp_LoadKeyBCE();

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPp_WriteKeyIndex")]
        public static extern UInt16 SSCPp_WriteKeyIndex(byte KeyType,
                                                        byte KeyIndex,
                                                        byte SectorNb,
                                                        byte GPB,
                                                        byte KeyMode,
                                                        byte IndexKeyA,
                                                        byte IndexKeyB);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPp_ReadBlockIndex")]
        public static extern UInt16 SSCPp_ReadBlockIndex(byte KeyType,
                                                         byte KeyIndex,
                                                         byte BlockNb,
                                                         [Out] byte[] Block);


        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPp_WriteBlockIndex")]
        public static extern UInt16 SSCPp_WriteBlockIndex(byte KeyType,
                                                          byte KeyIndex,
                                                          byte BlockNb,
                                                          [In] byte[] Block);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPp_ReadBlockIndexMacro")]
        public static extern UInt16 SSCPp_ReadBlockIndexMacro(byte KeyType,
                                                              byte KeyIndex,
                                                              byte BlockNb,
                                                              [Out] byte[] Block);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPp_WriteBlockIndexMacro")]
        public static extern UInt16 SSCPp_WriteBlockIndexMacro(byte KeyType,
                                                               byte KeyIndex,
                                                               byte BlockNb,
                                                               [In] byte[] Block);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPp_ReadSectorIndex")]
        public static extern UInt16 SSCPp_ReadSectorIndex(byte KeyType,
                                                          byte KeyIndex,
                                                          byte SectorNb,
                                                          byte IndexInSector,
                                                          [Out] byte[] Blocks);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPp_WriteSectorIndex")]
        public static extern UInt16 SSCPp_WriteSectorIndex(byte KeyType,
                                                           byte KeyIndex,
                                                           byte SectorNb,
                                                           byte IndexInSector,
                                                           [In] byte[] Blocks);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPp_ReadSectorIndexMacro")]
        public static extern UInt16 SSCPp_ReadSectorIndexMacro(byte KeyType,
                                                               byte KeyIndex,
                                                               byte SectorNb,
                                                               byte IndexInSector,
                                                               [Out] byte[] Blocks);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPp_WriteSectorIndexMacro")]
        public static extern UInt16 SSCPp_WriteSectorIndexMacro(byte KeyType,
                                                                byte KeyIndex,
                                                                byte SectorNb,
                                                                byte IndexInSector,
                                                                [In] byte[] Blocks);


        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPp_ReadBlockIndex_SL1")]
        public static extern UInt16 SSCPp_ReadBlockIndex_SL1(byte KeyType,
                                                             byte Crypto1KeyIndex,
                                                             byte AESKeyIndex,
                                                             byte BlockNb,
                                                             [Out] byte[] Block);


        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPp_WriteBlockIndex_SL1")]
        public static extern UInt16 SSCPp_WriteBlockIndex_SL1(byte KeyType,
                                                              byte Crypto1KeyIndex,
                                                              byte AESKeyIndex,
                                                              byte BlockNb,
                                                              [In] byte[] Block);

        /// Security Level 2

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPp_Authenticate_SL2")]
        public static extern UInt16 SSCPp_Authenticate_SL2(byte KeyType,
                                                           byte SectorNb,
                                                           [Out] byte[] Crypto1Key,  // 6 bytes Cryto1 (Mifare Classic) Key
                                                           [Out] byte[] AESkey);     // 16 bytes AES key



        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPp_ChangeAESKey_SL2")]
        public static extern UInt16 SSCPp_ChangeAESKey_SL2(byte KeyType,
                                                           byte SectorNb,         
                                                           [In] byte[] oldAESkey,      // 16 bytes old (current) AES key
                                                           [In] byte[] newAESkey);     // 16 bytes new AES key


        ///Security Level 3

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPp_PPS_SL3")]
        public static extern UInt16 SSCPp_PPS_SL3(byte BrIT,
                                                  byte BrTI);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPp_LoadKey_SL3")]
        public static extern UInt16 SSCPp_LoadKey_SL3(byte KeyType,
                                                      byte Save,
                                                      byte SectorNb,
                                                      [In] byte[] AESkey, // 16 bytes AES key to load
                                                      byte DiversifyKeys); // RFU set to 0    

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPp_Configure_SL3")]
        public static extern UInt16 SSCPp_Configure_SL3(UInt16 BNR,
                                                        [In] byte[] Value);     // 16 bytes value

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPp_FirstAuthenticate_SL3")]
        public static extern UInt16 SSCPp_FirstAuthenticate_SL3(byte KeyLocation,
                                                                byte KeyType,
                                                                byte SectorNb,
                                                                byte Index,
                                                                byte ReaderCapLen,
                                                                [In] byte[] ReaderCap);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPp_Authenticate_SL3")]
        public static extern UInt16 SSCPp_Authenticate_SL3(byte KeyLocation,
                                                           byte KeyType,
                                                           byte SectorNb,
                                                           byte Index);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPp_ReadBlocks_SL3")]
        public static extern UInt16 SSCPp_ReadBlocks_SL3(byte CommMode,
                                                         byte FirstBlock,
                                                         byte NBofBlocks,          // Max 16 blocks => 256 (16*16) bytes  
                                                         [Out] byte[] Blocks);     // 16*NBofBlocks bytes 

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPp_WriteBlocks_SL3")]
        public static extern UInt16 SSCPp_WriteBlocks_SL3(byte CommMode,
                                                          byte FirstBlock,
                                                          byte NBofBlocks,          // Max 16 blocks => 256 (16*16) bytes  
                                                          [In] byte[] Blocks);     // 16*NBofBlocks bytes 

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPp_WriteSectorKey_SL3")]
        public static extern UInt16 SSCPp_WriteSectorKey_SL3(byte KeyType,
                                                             byte SectorNb,
                                                             [In] byte[] newAESkey);     // 16 bytes AES key


        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPp_WriteSectorKeyIndex_SL3")]
        public static extern UInt16 SSCPp_WriteSectorKeyIndex_SL3(byte KeyType,
                                                                  byte SectorNb,
                                                                  byte GPB,
                                                                  byte IndexKeyA,
                                                                  byte IndexKeyB);   

        #endregion
      
      #region MIFARE® UltraLight /C functions

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPu_Scan")]
        public static extern UInt16 SSCPu_Scan(ref byte nbCard,
                                                    ref byte UIDLen,
                                                    [Out] byte[] UID);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPu_Release")]
        public static extern UInt16 SSCPu_Release();


        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPu_LoadKey")]
        public static extern UInt16 SSCPu_LoadKey(byte Save,
                                                       [In] byte[] DESkey1,
                                                       [In] byte[] DESkey2);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPu_Change3DESKey")]
        public static extern UInt16 SSCPu_Change3DESKey([In] byte[] DESkey1,
                                                        [In] byte[] DESkey2);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPu_Change3DESKeyIndex")]
        public static extern UInt16 SSCPu_Change3DESKeyIndex(byte NewKeyIndex);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPu_Authenticate")]
        public static extern UInt16 SSCPu_Authenticate(byte KeyLocation,
                                                            byte KeyIndex);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPu_Read4Pages")]
        public static extern UInt16 SSCPu_Read4Pages(byte FirstPageNb,
                                                          byte NbOf4Pages,
                                                         [Out] byte[] Pages4);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPu_WritePage")]
        public static extern UInt16 SSCPu_WritePage(byte FirstPageNb,
                                                         byte NbOfPage,
                                                         [In] byte[] Pages);

        //V1.1
        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPu_DiversificationKey")]
        public static extern UInt16 SSCPu_DiversificationKey(byte SavetoEEPROM,
                                                             [In] byte[] divKey);
        //V1.1


      #endregion

      #region CPS3 functions

        //////////// V1.1

        [DllImport(LIB_PATH_NAME, CharSet = CharSet.Unicode,EntryPoint = "SSCPc_GetErrorMsg")]
        public static extern UInt16 SSCPc_GetErrorMsg(UInt16 LID,      // Language ID
                                                      UInt16 Error,
                                                      ref string ErrorMsg);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPc_Select")]
        public static extern UInt16 SSCPc_Select(byte p1,      
                                                 byte p2,
                                                 byte dataLen,
                                                 [In] byte[] data,
                                                 ref byte datarecLen,
                                                 [Out] byte[] datarec,
                                                 ref UInt16 CPS3SW);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPc_Read")]
        public static extern UInt16 SSCPc_Read(byte p1,      
                                               byte p2,
                                               byte dataLen,
                                               [Out] byte[] dataRead,
                                               ref UInt16 CPS3SW);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPc_MSE_Set")]
        public static extern UInt16 SSCPc_MSE_Set(byte p1,     
                                                  byte p2,
                                                  byte Len,
                                                  [In] byte[] data,
                                                  ref UInt16 CPS3SW);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPc_GetChallenge")]
        public static extern UInt16 SSCPc_GetChallenge([Out] byte[] Rnd,      
                                                       ref UInt16 CPS3SW);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPc_External_Authenticate")]
        public static extern UInt16 SSCPc_External_Authenticate([Out] byte[] Rnd,      
                                                                [Out] byte[] data,
                                                                [Out] byte[] kenc,
                                                                [Out] byte[] kmac,
                                                                ref UInt16 CPS3SW);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPc_Update")]
        public static extern UInt16 SSCPc_Update(byte p1,
                                                 byte p2,
                                                 byte dataLen,
                                                 [In] byte[] data,
                                                 ref UInt16 CPS3SW);


        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPc_ClientServer_Authenticate")]
        public static extern UInt16 SSCPc_ClientServer_Authenticate(byte p1,
                                                                    byte p2,
                                                                    byte dataLen,
                                                                    [In] byte[] data,
                                                                    [Out] byte[] dataSign,
                                                                    ref UInt16 CPS3SW);

        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPc_Authenticate")]
        public static extern UInt16 SSCPc_Authenticate(byte location,
                                                       byte method,
                                                       byte indexenc,
                                                       byte indexmac,
                                                       ref UInt16 CPS3SW);
       
        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPc_LoadKey")]
        public static extern UInt16 SSCPc_LoadKey(byte save,
                                                  byte index,
                                                  [In] byte[] keyvalue,
                                                  ref UInt16 CPS3SW);
        
        [DllImport(LIB_PATH_NAME, EntryPoint = "SSCPc_Get_X509Certificate")]
        public static extern UInt16 SSCPc_Get_X509Certificate([Out] byte[] Certificate,
                                                              ref UInt16 CPS3SW);


      #endregion

        public SSCPlibMIFARE()
		{
			//
			// TODO: Add constructor logic here
			//
		}

    }
}
