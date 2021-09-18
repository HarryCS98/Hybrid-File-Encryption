using System;
using System.IO;
using System.Linq;
using System.Media;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace FileCrypter
{

    public partial class MainWindow
    {

        /*
         *Declare our global variables
         * 
         */
     
        
        //Workspace Location
        String WorkingDir = @Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory).ToString() + "\\";
        
        //Location for rsa public key export this can be changed on the settings menu
        String PubKeyFile = @Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "PublicKey.txt");
        String PrivateKeyFile = @Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "PrivateKey.txt");
        //Create a CspParameters object
        CspParameters cspp = new CspParameters();
        RSACryptoServiceProvider rsa;
        //Default AES Key
        string AESKey = "";


        /*
         *
         * Main Method (for the gui not for the entire application)
         * 
         */
       
        //Main method Adds all the compents in then runs the show files method which gets all the files from the directory that we have picked then displays them on
        //on the files page
        public MainWindow()
        {
            InitializeComponent();
            show_files();
        }

        
        
        /*
         *
         *Navigation button logic
         * 
         */
       
        //When the eye is clicked show tab 1
        private void btn_show_files(object sender, RoutedEventArgs e)
        {
            TabControl.SelectedIndex = 0;
            ListBox_Files.Items.Clear();
            show_files();
        }

        //When padlock is clicked show tab 2
        private void btn_encryption(object sender, RoutedEventArgs e)
        {
            TabControl.SelectedIndex = 1;
        }

        //When settings cog clicked show tab 3
        private void btn_settings(object sender, RoutedEventArgs e)
        {
            TabControl.SelectedIndex = 2;
        }
        
        
        
        /*
         *
         *Buttons that handle key related functions
         * 
         */
        
        
        
        //Generates and Encrypts our AES key using RSA so we can give out a public key which AES does not support
        private void buttonCreateAsmKeys_Click(object sender, System.EventArgs e)
        {
            Aes aes = Aes.Create();  
            aes.GenerateIV();  
            aes.GenerateKey();  
            AESKey = Encoding.Default.GetString(aes.Key);
            cspp.KeyContainerName = AESKey;
            rsa = new RSACryptoServiceProvider(cspp);
            rsa.PersistKeyInCsp = true;
            MessageBox.Show("RSA keys Generated");
            
        }
        
        
        void buttonExportPublicKey_Click(object sender, System.EventArgs e)
        {
            // Save the public key created by the RSA
            try
            {
                StreamWriter sw = new StreamWriter(PubKeyFile, false);
                sw.Write(rsa.ToXmlString(false));
                sw.Close();
            }
            catch(Exception ex)
            {
                MessageBox.Show("RSA keys not set press the Generate keys button");
            }
        }
        
        private void buttonExportPrivateKey(object sender, RoutedEventArgs e)
        {
           
            // Save the private key created by the RSA
            try
            {
                StreamWriter sw = new StreamWriter(PrivateKeyFile, false);
                sw.Write(rsa.ToXmlString(true));
                sw.Close();
            }
            catch(Exception ex)
            {
                MessageBox.Show("RSA keys not set press the Generate keys button");
            }
        }
        
        
        
        void buttonImportPrivateKey(object sender, System.EventArgs e)
        { 
            string name = "";
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.InitialDirectory = WorkingDir;
            if (openFileDialog1.ShowDialog() == true)
            {
                string fName = openFileDialog1.FileName;
                if (fName != null)
                {
                    FileInfo fInfo = new FileInfo(fName);
                    // Pass the file name without the path.
                    name = fInfo.FullName;
                }
            }
            StreamReader sr = new StreamReader(name);
            cspp.KeyContainerName = AESKey;
            rsa = new RSACryptoServiceProvider(cspp);
            string keytxt = sr.ReadToEnd();
            rsa.FromXmlString(keytxt);
            rsa.PersistKeyInCsp = true;
            MessageBox.Show("Private Key imported files can be encrypted and decrypted");
            sr.Close();
        }        
        
        void buttonImportPublicKey_Click(object sender, System.EventArgs e)
        { 
            string name = "";
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.InitialDirectory = WorkingDir;
            if (openFileDialog1.ShowDialog() == true)
            {
                string fName = openFileDialog1.FileName;
                if (fName != null)
                {
                    FileInfo fInfo = new FileInfo(fName);
                    // Pass the file name without the path.
                    name = fInfo.FullName;
                }
            }
            StreamReader sr = new StreamReader(name);
            cspp.KeyContainerName = AESKey;
            rsa = new RSACryptoServiceProvider(cspp);
            string keytxt = sr.ReadToEnd();
            rsa.FromXmlString(keytxt);
            rsa.PersistKeyInCsp = true;
            MessageBox.Show("Public key imported YOU CAN ONLY ENCRYPT FILES");
            sr.Close();
        }        
        
        
        
        /*
         *
         *Buttons that handle directory selection
         * 
         */
        
        //Allows us to set a custom working directory this is where encrypted and unencrypted files are kept
        private void buttonsetWorkingDir(object sender, System.EventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowserDialog.ShowDialog();
            String Dname = folderBrowserDialog.SelectedPath;
            WorkingDir = Dname + "\\";
            Label_FileDirectory.Content = "Location: " + WorkingDir;
        }

        
        //Sets the location which outputs our public key which we can then give to others to encrypt there files
        private void buttonsetpublickeyexportlocation(object sender, System.EventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowserDialog.ShowDialog();
            String Dname = folderBrowserDialog.SelectedPath;
            PubKeyFile  = Dname + "\\" + "PublicKey.txt";
            PrivateKeyFile = Dname + "\\" + "PrivateKey.txt";
            String label_string = PubKeyFile.Replace("PublicKey.txt", "");
            Label_ExportKeyDirectory.Content = "Location: " + label_string ;
        }

        
        
        
        /*
         *
         *Encrypt and decrypt button methods
         * 
         */
        
        //Encrypts files method first checks a key has been set then gets the file name from the button then passes this to the encrypt file method which then 
        //Encrypts the files and deletes the unencrypted one
        private void buttonEncryptFile_Click(object sender, System.EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (rsa == null)
            {
                MessageBox.Show("RSA Keys not set.");
            }
            else
            {
                // Display a dialog box to select a file to encrypt.
                Button button = (Button) sender;
                String id = button.Uid;
                id = WorkingDir + id;

                id.Replace(".", "_");
                id = id + ".";
                EncryptFile(id);
                // Change the file's extension to ".enc"
            }
            ListBox_Files.Items.Clear();
            show_files();
        }

        
        private void buttonDecryptFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog2 = new OpenFileDialog();
            if (rsa == null)
            {
                MessageBox.Show("Key not set.");
            }
            else
            {
                // Display a dialog box to select the encrypted file.
                Button button = (Button) sender;
                String id = button.Uid;
                DecryptFile(id);
            }
            ListBox_Files.Items.Clear();
            show_files();
        }
        
        
        
        
        /*
         *
         * The actual Encrypt and Decrypt methods that do the encrypting and Decrypting trigger by the methods above
         * 
         */
        
         private void EncryptFile(string inFile)
        {
            // Create instance of Aes for
            // symmetric encryption of the data.
            Aes aes = Aes.Create();
            ICryptoTransform transform = aes.CreateEncryptor();

            // Use RSACryptoServiceProvider to
            // encrypt the AES key.
            // rsa is previously instantiated:
            //    rsa = new RSACryptoServiceProvider(cspp);
            byte[] keyEncrypted = rsa.Encrypt(aes.Key, false);

            // Create byte arrays to contain
            // the length values of the key and IV.
            byte[] LenK = new byte[4];
            byte[] LenIV = new byte[4];

            int lKey = keyEncrypted.Length;
            LenK = BitConverter.GetBytes(lKey);
            int lIV = aes.IV.Length;
            LenIV = BitConverter.GetBytes(lIV);
  
            int startFileName = inFile.LastIndexOf("\\") + 1;
            //Change the file's extension to ".enc"
            string outFile = WorkingDir + inFile.Substring(startFileName, inFile.LastIndexOf(".") - startFileName) + ".enc";

            
            
           
            using (FileStream outFs = new FileStream(outFile, FileMode.Create))
            {
                outFs.Write(LenK, 0, 4);
                outFs.Write(LenIV, 0, 4);
                outFs.Write(keyEncrypted, 0, lKey);
                outFs.Write(aes.IV, 0, lIV);

                // Now write the cipher text using
                // a CryptoStream for encrypting.
                using (CryptoStream outStreamEncrypted = new CryptoStream(outFs, transform, CryptoStreamMode.Write))
                {
                    // By encrypting a chunk at
                    // a time, you can save memory
                    // and accommodate large files.
                    int count = 0;
                    int offset = 0;

                    // blockSizeBytes can be any arbitrary size.
                    int blockSizeBytes = aes.BlockSize / 8;
                    byte[] data = new byte[blockSizeBytes];
                    int bytesRead = 0;

                    using (FileStream inFs = new FileStream(inFile, FileMode.Open))
                    {
                        do
                        {
                            count = inFs.Read(data, 0, blockSizeBytes);
                            offset += count;
                            outStreamEncrypted.Write(data, 0, count);
                            bytesRead += blockSizeBytes;
                        } while (count > 0);

                        inFs.Close();
                    }

                    outStreamEncrypted.FlushFinalBlock();
                    outStreamEncrypted.Close();
                }

                outFs.Close();
              
            }
            if (File.Exists(Path.Combine(WorkingDir, inFile)))    
            {    
                // If file found, delete it    
                File.Delete(Path.Combine(WorkingDir, inFile));
            }    
            else MessageBox.Show("File not found"); 
        }
      

        private void DecryptFile(string inFile)
        {
            // Create instance of Aes for
            // symetric decryption of the data.
            Aes aes = Aes.Create();

            // Create byte arrays to get the length of
            // the encrypted key and IV.
            // These values were stored as 4 bytes each
            // at the beginning of the encrypted package.
            byte[] LenK = new byte[4];
            byte[] LenIV = new byte[4];

            // Construct the file name for the decrypted file.
           // string outFile = WorkingDir + inFile.Substring(0, inFile.LastIndexOf(".") - 4);
           string outFile = WorkingDir + inFile.Substring(0, inFile.LastIndexOf("."));

            // Use FileStream objects to read the encrypted
            // file (inFs) and save the decrypted file (outFs).
            using (FileStream inFs = new FileStream(WorkingDir + inFile, FileMode.Open))
            {
                inFs.Seek(0, SeekOrigin.Begin);
                inFs.Seek(0, SeekOrigin.Begin);
                inFs.Read(LenK, 0, 3);
                inFs.Seek(4, SeekOrigin.Begin);
                inFs.Read(LenIV, 0, 3);

                // Convert the lengths to integer values.
                int lenK = BitConverter.ToInt32(LenK, 0);
                int lenIV = BitConverter.ToInt32(LenIV, 0);

                // Determine the start postition of
                // the ciphter text (startC)
                // and its length(lenC).
                int startC = lenK + lenIV + 8;
                int lenC = (int) inFs.Length - startC;

                // Create the byte arrays for
                // the encrypted Aes key,
                // the IV, and the cipher text.
                byte[] KeyEncrypted = new byte[lenK];
                byte[] IV = new byte[lenIV];

                // Extract the key and IV
                // starting from index 8
                // after the length values.
                inFs.Seek(8, SeekOrigin.Begin);
                inFs.Read(KeyEncrypted, 0, lenK);
                inFs.Seek(8 + lenK, SeekOrigin.Begin);
                inFs.Read(IV, 0, lenIV);
                Directory.CreateDirectory(WorkingDir);
                // Use RSACryptoServiceProvider
                // to decrypt the AES key.
                byte[] KeyDecrypted = rsa.Decrypt(KeyEncrypted, false);

                // Decrypt the key.
                ICryptoTransform transform = aes.CreateDecryptor(KeyDecrypted, IV);

                // Decrypt the cipher text from
                // from the FileSteam of the encrypted
                // file (inFs) into the FileStream
                // for the decrypted file (outFs).
                using (FileStream outFs = new FileStream(outFile, FileMode.Create))
                {
                    int count = 0;
                    int offset = 0;

                    // blockSizeBytes can be any arbitrary size.
                    int blockSizeBytes = aes.BlockSize / 8;
                    byte[] data = new byte[blockSizeBytes];

                    // By decrypting a chunk a time,
                    // you can save memory and
                    // accommodate large files.

                    // Start at the beginning
                    // of the cipher text.
                    inFs.Seek(startC, SeekOrigin.Begin);
                    using (CryptoStream outStreamDecrypted = new CryptoStream(outFs, transform, CryptoStreamMode.Write))
                    {
                        do
                        {
                            count = inFs.Read(data, 0, blockSizeBytes);
                            offset += count;
                            outStreamDecrypted.Write(data, 0, count);
                        } while (count > 0);

                        outStreamDecrypted.FlushFinalBlock();
                        outStreamDecrypted.Close();
                    }

                    outFs.Close();
                }

                inFs.Close();
            }
            if (File.Exists(Path.Combine(WorkingDir, inFile)))    
            {    
                // If file found, delete it    
                File.Delete(Path.Combine(WorkingDir, inFile));
                MessageBox.Show("File Decrypted.");    
            }    
            else MessageBox.Show("File not found"); 
        }
        
        
        
        
        /*
         *
         * Graphical methods these handle GUI elements but in code rather then in our xaml document 
         * 
         */

        private void show_files()
        {
            //Create the items we are going to add into the application each will be created for each file.
            StackPanel stackPanel;
            TextBlock textBlock;
            Image Img;
            BitmapImage imgsrc;
            String temp;
            Button Encrypt,Decrypt;
            
            
            
            //Get all the files in the directory
            string[] A = Directory.GetFiles(WorkingDir, "*", SearchOption.TopDirectoryOnly);

            //Loop through all the files in the directory (that we are storing in the array called a
            for (int i = 0; i < A.Length; i++)
            {
                //If the files is a txt file we are using this for a test later we will use enc files as that is an encrypted file
                // if (A[i].Contains("env"))

                //Initialise the string called temp and then give it the name value for the current file 
                temp = A[i];
                //We can now use the Dir string variable that we created earlier to clean this variable up so it only has the file name left 
                temp = temp.Replace(WorkingDir, "");
                //Initialise the stack panel variable and set its Orientation to horizontal this will allow us to have a picture then the file name
                stackPanel = new StackPanel() {Orientation = Orientation.Horizontal};
                
                //Initialise the textblock that we will put our file name in
                textBlock = new TextBlock();
                //Initialise the Img variable this is what we will use to display our image later
                Img = new Image();

                if (A[i].Contains("enc"))
                {
                    //Set the img src to the locked pic
                    imgsrc = new BitmapImage(new Uri("img/004-lock.png", UriKind.Relative));
                }
                else
                {
                    //Set the img src to the unlocked pic
                    imgsrc = new BitmapImage(new Uri("img/004-lock2.png", UriKind.Relative));
                }

                //Set the textblocks vertical alignment to center so it sits to the right and center of the image
                textBlock.VerticalAlignment = VerticalAlignment.Center;
                //Set the source of our image using the imgsrc variable that we initialised earlier
                Img.Source = imgsrc;
                //Set the img height to 50
                Img.Height = 50;
                //Set the img width to 50
                Img.Width = 50;
                //Set the Text i.e the content of our textblock to our file name that we cleaned up earlier
                textBlock.Text = temp;
                //Set text colour to white
                textBlock.Foreground = Brushes.White;
                //Add the stack panel to our listbox that is created in xaml
                ListBox_Files.Items.Add(stackPanel);
                //Add the img and text to the stackpanel each line is a new stackpanel this will give us a img and file name on each row
                stackPanel.Children.Add(Img);
                stackPanel.Children.Add(textBlock);
                if (A[i].Contains("enc") && rsa != null && rsa.PublicOnly == false)
                {
                    //DeCrypt = new Button();
                    Decrypt = new Button();
                    Decrypt.Click += new RoutedEventHandler(buttonDecryptFile_Click);
                    Decrypt.Content = "Decrypt";
                    Decrypt.Uid = temp;
                    Decrypt.Margin = new Thickness(20,0,0,0);
                    Decrypt.Background = new SolidColorBrush(Colors.Red);
                    Decrypt.BorderBrush = new SolidColorBrush(Colors.Black);
                    Decrypt.Height = 30;
                    stackPanel.Children.Add(Decrypt);
                }
                else if(rsa!=null && !A[i].Contains(".enc"))
                {
                    //Generate a new Encrypt and DeCrypt button
                    Encrypt = new Button();
                    Encrypt.Click += new RoutedEventHandler(buttonEncryptFile_Click);
                    Encrypt.Content = "Encrypt";
                    Encrypt.Uid = temp;
                    Encrypt.Margin = new Thickness(20,0,0,0);
                    Encrypt.Background = new SolidColorBrush(Colors.Green);
                    Encrypt.BorderBrush = new SolidColorBrush(Colors.Black);
                    Encrypt.Height = 30;
                    
                    stackPanel.Children.Add(Encrypt);
                }

                stackPanel.Margin = new Thickness(0, 20, 0, 0);
            }
        }



       
    }
}