using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Windows.Forms;
using System.Threading;
using System.Reflection;
using System.Diagnostics;

namespace BNS_Tools
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow currentMainWindow;

        private double h = 115;
        private int ar = 6;
        private int dar = 1;

        public MainWindow()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (s, args) =>
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                List<string> embeddedResources = new List<string>(assembly.GetManifestResourceNames());
                string assemblyName = new AssemblyName(args.Name).Name;
                string fileName = string.Format("{0}.dll", assemblyName);
                string resourceName = embeddedResources.Where(ern => ern.EndsWith(fileName)).FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(resourceName))
                {
                    using (var stream = assembly.GetManifestResourceStream(resourceName))
                    {
                        Byte[] assemblyData = new Byte[stream.Length];
                        stream.Read(assemblyData, 0, assemblyData.Length);
                        var test = Assembly.Load(assemblyData);
                        string namespace_ = test.GetTypes().Where(t => t.Name == assemblyName).Select(t => t.Namespace).FirstOrDefault();
                        return Assembly.Load(assemblyData);
                    }
                }

                return null;
            };

            currentMainWindow = this;

            InitializeComponent();

            body.Height -= h;

            textBlock.Visibility = Visibility.Hidden;

            SearchGame();

            radio_32b.IsChecked = true;

            Start();
        }

        private void Start()
        {
            Thread st = new Thread(timer);
            st.IsBackground = true;
            st.Start();
        }

        private void client_search()
        {
            bool _is = false;
            bool _is2 = false;

            string path = game_path.Replace(@"/", @"\") + @"\bin\Frost\bns_platform_x";
            bool frost64 = File.Exists(path + "64");

            Process[] procs = Process.GetProcessesByName("client");
            foreach (Process pr in procs)
            {
                _is = true;
                button.IsEnabled = false;
                textBlock.Text = "Закройте игру";
                textBlock.Visibility = Visibility.Visible;

                radio_32b.Visibility = Visibility.Hidden;
                radio_64b.Visibility = Visibility.Hidden;
                button_close.Visibility = Visibility.Visible;

                texture_off.IsEnabled = false;
                texture_on.IsEnabled = false;
            }

            if (!_is)
            {
                button.IsEnabled = true;                
                textBlock.Visibility = Visibility.Hidden;
                if (!_is2)
                {
                    texture_off.IsEnabled = true;
                    texture_on.IsEnabled = true;
                }
            }
        }

        private void timer()
        {
            DateTime dt = new DateTime();
            while (true)
            {
                Dispatcher.Invoke(() => { client_search(); texture_check(); });

                dt = DateTime.Now;
                while ((DateTime.Now - dt).TotalMilliseconds < 666)
                {
                    Thread.Sleep(250);
                }

                if (patching) break;
            }
        }

        private void texture_check()
        {
            string folder = game_path.Replace(@"/", @"\") + @"\contents\bns\CookedPC\";

            if (File.Exists(folder + "00007917.bak"))
            {
                chbox_kot.IsChecked = true;
                chbox_kot.IsEnabled = false;
            }
            else chbox_kot.IsEnabled = true;

            if (File.Exists(folder + "00007916.bak"))
            {
                chbox_sin.IsChecked = true;
                chbox_sin.IsEnabled = false;
            }
            else chbox_sin.IsEnabled = true;

            if (File.Exists(folder + "00007915.bak"))
            {
                chbox_gun.IsChecked = true;
                chbox_gun.IsEnabled = false;
            }
            else chbox_gun.IsEnabled = true;

            if (File.Exists(folder + "00007914.bak"))
            {
                chbox_des.IsChecked = true;
                chbox_des.IsEnabled = false;
            }
            else chbox_des.IsEnabled = true;

            if (File.Exists(folder + "00007913.bak"))
            {
                chbox_fm.IsChecked = true;
                chbox_fm.IsEnabled = false;
            }
            else chbox_fm.IsEnabled = true;

            if (File.Exists(folder + "00007912.bak"))
            {
                chbox_kfm.IsChecked = true;
                chbox_kfm.IsEnabled = false;
            }
            else chbox_kfm.IsEnabled = true;

            if (File.Exists(folder + "00007911.bak"))
            {
                chbox_bm.IsChecked = true;
                chbox_bm.IsEnabled = false;
            }
            else chbox_bm.IsEnabled = true;

            if (File.Exists(folder + "00018601.bak"))
            {
                chbox_lsm.IsChecked = true;
                chbox_lsm.IsEnabled = false;
            }
            else chbox_lsm.IsEnabled = true;

            if (File.Exists(folder + "00023439.bak"))
            {
                chbox_wl.IsChecked = true;
                chbox_wl.IsEnabled = false;
            }
            else chbox_wl.IsEnabled = true;

            if (File.Exists(folder + "00034408.bak"))
            {
                chbox_sf.IsChecked = true;
                chbox_sf.IsEnabled = false;
            }
            else chbox_sf.IsEnabled = true;

            if (File.Exists(folder + "00056126.bak"))//00056566
            {
                chbox_war.IsChecked = true;
                chbox_war.IsEnabled = false;
            }
            else chbox_war.IsEnabled = true;

        }

        static RegistryKey key_soft = Registry.LocalMachine.OpenSubKey("SOFTWARE");
        RegistryKey key_game;

        private string game_path = "";
        private string folder_path = "";
        private bool bit64 = false;
        private bool xml_plus = false;

        private CultureInfo originalCulture;

        private bool verifyFolder()
        {
            folder_path = game_path.Replace(@"/", @"\") + @"\contents\Local\INNOVA\data\";

            return Directory.Exists(folder_path);
        }

        private void SearchGame()
        {
            try
            {
                RegistryKey key = key_soft.OpenSubKey("4game");
                if (key == null)
                {
                    key = key_soft.OpenSubKey("WOW6432Node");
                    key = key.OpenSubKey("4game");
                }
                key = key.OpenSubKey("4gameservice");
                key = key.OpenSubKey("Games");
                key_game = key.OpenSubKey("Blade and Soul");
                game_path = key_game.GetValue("path").ToString();
            }
            catch
            {
                game_path = @"C:\Games\Blade and Soul\";
            }

            textBox.Text = game_path.Replace(@"/", @"\");
        }

        public void Compiler()
        {
            string bits = "";
            if (bit64) bits = "64";

            string folderpath = folder_path + "xml" + bits + ".dat.files";

            Dispatcher.Invoke(() => { textBlock.Text = "Compress..."; });

            BNSDat.BNSDat BnsDat = new BNSDat.BNSDat();
            BnsDat.Compress(folderpath, bit64);

            if (Directory.Exists(folder_path + "xml" + bits + ".dat.files")) { Directory.Delete(folder_path + "xml" + bits + ".dat.files", true); }

            Dispatcher.Invoke(() => { textBlock.Text = "OK"; });
        }

        public void Extractor()
        {
            string bits = "";
            if (bit64) bits = "64";

            string filepath = folder_path + "xml" + bits + ".dat";

            if (Directory.Exists(folder_path + "xml" + bits + ".dat.files")) { Directory.Delete(folder_path + "xml" + bits + ".dat.files", true); }

            if (File.Exists(filepath))
            {
                BNSDat.BNSDat BnsDat = new BNSDat.BNSDat();
                BnsDat.Extract(filepath, bit64);
            }
        }

        private void DPSturnOn()
        {
            Extractor();

            string bits = "";
            if (bit64) bits = "64";

            string directory = folder_path + "xml" + bits + @".dat.files\client.config2.xml";

            StreamReader reader = new StreamReader(directory);
            string input = reader.ReadToEnd();
            reader.Close();

            using (StreamWriter writer = new StreamWriter(directory))
            {
                string output = edit_xml(input);

                if (xml_plus)
                {
                    output = edit_xml_plus(output);
                }

                writer.Write(output);

                writer.Close();
            }

            Compiler();

            //_exit();
            Dispatcher.Invoke(() =>
            {
                texture_on.IsEnabled = true;
                texture_off.IsEnabled = true;
            });
        }

        private string edit_xml(string input)
        {
            string word = "option name=\"showtype-party-6-dungeon-and-cave\" value=\"1\"";
            string replacement = "option name=\"showtype-party-6-dungeon-and-cave\" value=\"2\"";

            string output = input.Replace(word, replacement);

            word = "option name=\"showtype-public-zone\" value=\"0\"";
            replacement = "option name=\"showtype-public-zone\" value=\"2\"";
            output = output.Replace(word, replacement);

            word = "option name=\"showtype-field-zone\" value=\"0\"";
            replacement = "option name=\"showtype-field-zone\" value=\"2\"";
            output = output.Replace(word, replacement);

            word = "option name=\"showtype-classic-field-zone\" value=\"0\"";
            replacement = "option name=\"showtype-classic-field-zone\" value=\"2\"";
            output = output.Replace(word, replacement);

            word = "option name=\"showtype-faction-battle-field-zone\" value=\"0\"";
            replacement = "option name=\"showtype-faction-battle-field-zone\" value=\"2\"";
            output = output.Replace(word, replacement);

            word = "option name=\"showtype-jackpot-boss-zone\" value=\"0\"";
            replacement = "option name=\"showtype-jackpot-boss-zone\" value=\"2\"";
            output = output.Replace(word, replacement);

            return output;
        }

        private string edit_xml_plus(string input)
        {
            string word = "option name=\"pending-time\" value=\"0.300000\"";
            string replacement = "option name=\"pending-time\" value=\"0.010000\"";

            string output = input.Replace(word, replacement);

            word = "option name=\"pending-key-tick-time\" value=\"0.25\"";
            replacement = "option name=\"pending-key-tick-time\" value=\"0.10\"";
            output = output.Replace(word, replacement);

            word = "option name=\"pressed-key-tick-time\" value=\"0.25\"";
            replacement = "option name=\"pressed-key-tick-time\" value=\"0.10\"";
            output = output.Replace(word, replacement);

            word = "option name=\"ignore-mouse-press-time\" value=\"1.000000\"";
            replacement = "option name=\"ignore-mouse-press-time\" value=\"0.010000\"";
            output = output.Replace(word, replacement);

            /*
            word = "option name=\"distance2\" value=\"40.000000\"";            
            replacement = "option name=\"distance2\" value=\"320.000000\"";
            output = output.Replace(word, replacement);
            //али кота?
            */

            word = "option name=\"other-hide-show-1\" value=\"00007916.hide_enemy5\"";
            replacement = "option name=\"other-hide-show-1\" value=\"00007916.hide\"";
            output = output.Replace(word, replacement);

            word = "option name=\"other-hide-show-2\" value=\"00007916.hide_enemy4\"";
            replacement = "option name=\"other-hide-show-2\" value=\"00007916.hide\"";
            output = output.Replace(word, replacement);

            word = "option name=\"other-hide-show-3\" value=\"00007916.hide_enemy3\"";
            replacement = "option name=\"other-hide-show-3\" value=\"00007916.hide\"";
            output = output.Replace(word, replacement);

            word = "option name=\"other-hide-show-4\" value=\"00007916.hide_enemy2\"";
            replacement = "option name=\"other-hide-show-4\" value=\"00007916.hide\"";
            output = output.Replace(word, replacement);

            word = "option name=\"other-hide-show-5\" value=\"00007916.hide_enemy1\"";
            replacement = "option name=\"other-hide-show-5\" value=\"00007916.hide\"";
            output = output.Replace(word, replacement);
            //инвизы

            word = "option name=\"hidden-pc-name-rating\" value=\"160000\"";
            replacement = "option name=\"hidden-pc-name-rating\" value=\"120000\"";
            output = output.Replace(word, replacement);
            //скрытие имени арены серебро

            word = "option name=\"lobby-gamemode-change-interval\" value=\"3\"";
            replacement = "option name=\"lobby-gamemode-change-interval\" value=\"1\"";
            output = output.Replace(word, replacement);

            word = "option name=\"lobby-arena-match-restart-interval\" value=\"3\"";
            replacement = "option name=\"lobby-arena-match-restart-interval\" value=\"1\"";
            output = output.Replace(word, replacement);
            //кд реса на арене

            word = "option name=\"lobby-arena-match-reready-interval\" value=\"3\"";
            replacement = "option name=\"lobby-arena-match-reready-interval\" value=\"1\"";
            output = output.Replace(word, replacement);
            //кд реса на арене

            return output;
        }

        private bool patching = false;

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (verifyFolder())
            {
                patching = true;

                texture_off.IsEnabled = false;
                texture_on.IsEnabled = false;

                game_path = textBox.Text;
                textBlock.Text = "Extracting...";
                textBlock.Visibility = Visibility.Visible;
                button.IsEnabled = false;
                radio_32b.IsEnabled = false;
                radio_64b.IsEnabled = false;
                button_folder.IsEnabled = false;
                checkbox_xml_plus.IsEnabled = false;
                Thread t = new Thread(DPSturnOn);
                t.IsBackground = true;
                t.Start();
            }
            else
            {
                Dispatcher.Invoke(() => { textBlock.Text = "Path error"; });
                System.Windows.MessageBox.Show("Не корректно указан адрес");
                button.IsEnabled = true;
                radio_32b.IsEnabled = true;
                radio_64b.IsEnabled = true;
                button_folder.IsEnabled = true;
                checkbox_xml_plus.IsEnabled = true;
            }
        }

        private void radio_32b_Checked(object sender, RoutedEventArgs e)
        {
            radio_64b.IsChecked = false;
            bit64 = false;
        }

        private void radio_64b_Checked(object sender, RoutedEventArgs e)
        {
            radio_32b.IsChecked = false;
            bit64 = true;
        }

        private void button_folder_Click(object sender, RoutedEventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (!string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    textBox.Text = fbd.SelectedPath;
                }
            }
        }

        public void SortOutputHandler(int value)
        {
            Dispatcher.Invoke(() => { progressbar.Value = value; });
        }

        private void _exit()
        {
            DateTime dt = new DateTime();
            dt = DateTime.Now;
            while ((DateTime.Now - dt).TotalMilliseconds < 3000)
            {
                Thread.Sleep(500);
            }

            Dispatcher.Invoke(() => { this.Close(); });
        }

        private void checkbox_xml_plus_Checked(object sender, RoutedEventArgs e)
        {
            xml_plus = true;
        }

        private void checkbox_xml_plus_Unchecked(object sender, RoutedEventArgs e)
        {
            xml_plus = false;
        }

        private void texture_off_Click(object sender, RoutedEventArgs e)
        {
            string folder = game_path.Replace(@"/", @"\") + @"\contents\bns\CookedPC\";
            progressbar.Value = progressbar.Minimum;
            textBlock.Visibility = Visibility.Hidden;

            if ((bool)chbox_kot.IsChecked)
                if (File.Exists(folder + "00007917.upk"))
                {
                    File.Copy(folder + "00007917.upk", folder + "00007917.bak", true);
                    File.Delete(folder + "00007917.upk");
                }

            if ((bool)chbox_sin.IsChecked)
                if (File.Exists(folder + "00007916.upk"))
                {
                    File.Copy(folder + "00007916.upk", folder + "00007916.bak", true);
                    File.Delete(folder + "00007916.upk");
                }

            if ((bool)chbox_gun.IsChecked)
                if (File.Exists(folder + "00007915.upk"))
                {
                    File.Copy(folder + "00007915.upk", folder + "00007915.bak", true);
                    File.Delete(folder + "00007915.upk");
                }

            if ((bool)chbox_des.IsChecked)
                if (File.Exists(folder + "00007914.upk"))
                {
                    File.Copy(folder + "00007914.upk", folder + "00007914.bak", true);
                    File.Delete(folder + "00007914.upk");
                }

            if ((bool)chbox_fm.IsChecked)
                if (File.Exists(folder + "00007913.upk"))
                {
                    File.Copy(folder + "00007913.upk", folder + "00007913.bak", true);
                    File.Delete(folder + "00007913.upk");
                }

            if ((bool)chbox_kfm.IsChecked)
                if (File.Exists(folder + "00007912.upk"))
                {
                    File.Copy(folder + "00007912.upk", folder + "00007912.bak", true);
                    File.Delete(folder + "00007912.upk");
                }

            if ((bool)chbox_bm.IsChecked)
                if (File.Exists(folder + "00007911.upk"))
                {
                    File.Copy(folder + "00007911.upk", folder + "00007911.bak", true);
                    File.Delete(folder + "00007911.upk");
                }

            if ((bool)chbox_lsm.IsChecked)
                if (File.Exists(folder + "00018601.upk"))
                {
                    File.Copy(folder + "00018601.upk", folder + "00018601.bak", true);
                    File.Delete(folder + "00018601.upk");
                }

            if ((bool)chbox_wl.IsChecked)
                if (File.Exists(folder + "00023439.upk"))
                {
                    File.Copy(folder + "00023439.upk", folder + "00023439.bak", true);
                    File.Delete(folder + "00023439.upk");
                }

            if ((bool)chbox_sf.IsChecked)
                if (File.Exists(folder + "00034408.upk"))
                {
                    File.Copy(folder + "00034408.upk", folder + "00034408.bak", true);
                    File.Delete(folder + "00034408.upk");
                }

            if ((bool)chbox_war.IsChecked)
            {
                if (File.Exists(folder + "00056126.upk"))
                {
                    File.Copy(folder + "00056126.upk", folder + "00056126.bak", true);
                    File.Delete(folder + "00056126.upk");
                }

                if (File.Exists(folder + "00056566.upk"))
                {
                    File.Copy(folder + "00056566.upk", folder + "00056566.bak", true);
                    File.Delete(folder + "00056566.upk");
                }
            }

            progressbar.Value = progressbar.Maximum;
            textBlock.Text = "OK";
            textBlock.Visibility = Visibility.Visible;
        }

        private void texture_on_Click(object sender, RoutedEventArgs e)
        {
            string folder = game_path.Replace(@"/", @"\") + @"\contents\bns\CookedPC\";
            progressbar.Value = progressbar.Minimum;
            textBlock.Visibility = Visibility.Hidden;

            try
            {
                if (File.Exists(folder + "00007917.bak"))
                {
                    File.Copy(folder + "00007917.bak", folder + "00007917.upk", true);
                    File.Delete(folder + "00007917.bak");
                }
            }
            catch { }

            try
            {
                if (File.Exists(folder + "00007916.bak"))
                {
                    File.Copy(folder + "00007916.bak", folder + "00007916.upk", true);
                    File.Delete(folder + "00007916.bak");
                }
            }
            catch { }

            try
            {
                if (File.Exists(folder + "00007915.bak"))
                {
                    File.Copy(folder + "00007915.bak", folder + "00007915.upk", true);
                    File.Delete(folder + "00007915.bak");
                }
            }
            catch { }

            try
            {
                if (File.Exists(folder + "00007914.bak"))
                {
                    File.Copy(folder + "00007914.bak", folder + "00007914.upk", true);
                    File.Delete(folder + "00007914.bak");
                }
            }
            catch { }

            try
            {
                if (File.Exists(folder + "00007913.bak"))
                {
                    File.Copy(folder + "00007913.bak", folder + "00007913.upk", true);
                    File.Delete(folder + "00007913.bak");
                }
            }
            catch { }

            try
            {
                if (File.Exists(folder + "00007912.bak"))
                {
                    File.Copy(folder + "00007912.bak", folder + "00007912.upk", true);
                    File.Delete(folder + "00007912.bak");
                }
            }
            catch { }

            try
            {
                if (File.Exists(folder + "00007911.bak"))
                {
                    File.Copy(folder + "00007911.bak", folder + "00007911.upk", true);
                    File.Delete(folder + "00007911.bak");
                }
            }
            catch { }

            try
            {
                if (File.Exists(folder + "00018601.bak"))
                {
                    File.Copy(folder + "00018601.bak", folder + "00018601.upk", true);
                    File.Delete(folder + "00018601.bak");
                }
            }
            catch { }

            try
            {
                if (File.Exists(folder + "00023439.bak"))
                {
                    File.Copy(folder + "00023439.bak", folder + "00023439.upk", true);
                    File.Delete(folder + "00023439.bak");
                }
            }
            catch { }

            try
            {
                if (File.Exists(folder + "00034408.bak"))
                {
                    File.Copy(folder + "00034408.bak", folder + "00034408.upk", true);
                    File.Delete(folder + "00034408.bak");
                }
            }
            catch { }

            try
            {
                if (File.Exists(folder + "00056126.bak"))
                {
                    File.Copy(folder + "00056126.bak", folder + "00056126.upk", true);
                    File.Delete(folder + "00056126.bak");
                }
            }
            catch { }

            try
            {
                if (File.Exists(folder + "00056566.bak"))
                {
                    File.Copy(folder + "00056566.bak", folder + "00056566.upk", true);
                    File.Delete(folder + "00056566.bak");
                }
            }
            catch { }

            progressbar.Value = progressbar.Maximum;
            textBlock.Text = "OK";
            textBlock.Visibility = Visibility.Visible;
        }

        private void button_body_Click(object sender, RoutedEventArgs e)
        {
            ar = ar - dar;
            button_body.Content = ar;
            dar = -dar;

            body.Height += h;
            h = -h;
        }

        private void button_close_Click(object sender, RoutedEventArgs e)
        {
            Process[] _procs = Process.GetProcessesByName("client");

            foreach (Process pr in _procs)
            {
                Process.GetProcessById(pr.Id).Kill();
            }
        }
    }
}




namespace BNSDat
{
    enum BXML_TYPE
    {
        BXML_PLAIN,
        BXML_BINARY,
        BXML_UNKNOWN
    };

    class BPKG_FTE
    {
        public int FilePathLength;
        public string FilePath;
        public byte Unknown_001;
        public bool IsCompressed;
        public bool IsEncrypted;
        public byte Unknown_002;
        public int FileDataSizeUnpacked;
        public int FileDataSizeSheared; // without padding for AES
        public int FileDataSizeStored;
        public int FileDataOffset; // (relative) offset
        public byte[] Padding;

    }

    public class BNSDat
    {

        public string AES_KEY = "bns_obt_kr_2014#";

        public byte[] XOR_KEY = new byte[16] { 164, 159, 216, 179, 246, 142, 57, 194, 45, 224, 97, 117, 92, 75, 26, 7 };

        private byte[] Decrypt(byte[] buffer, int size)
        {
            // AES requires buffer to consist of blocks with 16 bytes (each)
            // expand last block by padding zeros if required...
            // -> the encrypted data in BnS seems already to be aligned to blocks
            int AES_BLOCK_SIZE = AES_KEY.Length;
            int sizePadded = size + AES_BLOCK_SIZE;
            byte[] output = new byte[sizePadded];
            byte[] tmp = new byte[sizePadded];
            buffer.CopyTo(tmp, 0);
            buffer = null;

            Rijndael aes = Rijndael.Create();
            aes.Mode = CipherMode.ECB;
            ICryptoTransform decrypt = aes.CreateDecryptor(Encoding.ASCII.GetBytes(AES_KEY), new byte[16]);
            decrypt.TransformBlock(tmp, 0, sizePadded, output, 0);
            tmp = output;
            output = new byte[size];
            Array.Copy(tmp, 0, output, 0, size);
            tmp = null;

            return output;
        }

        private byte[] Deflate(byte[] buffer, int sizeCompressed, int sizeDecompressed)
        {
            byte[] tmp = Ionic.Zlib.ZlibStream.UncompressBuffer(buffer);

            if (tmp.Length != sizeDecompressed)
            {
                byte[] tmp2 = new byte[sizeDecompressed];

                if (tmp.Length > sizeDecompressed)
                    Array.Copy(tmp, 0, tmp2, 0, sizeDecompressed);
                else
                    Array.Copy(tmp, 0, tmp2, 0, tmp.Length);
                tmp = tmp2;
                tmp2 = null;
            }
            return tmp;
        }

        private byte[] Unpack(byte[] buffer, int sizeStored, int sizeSheared, int sizeUnpacked, bool isEncrypted, bool isCompressed)
        {
            byte[] output = buffer;

            if (isEncrypted)
            {
                output = Decrypt(output, sizeStored);
            }

            if (isCompressed)
            {
                output = Deflate(output, sizeSheared, sizeUnpacked);
            }

            // neither encrypted, nor compressed -> raw copy
            if (output == buffer)
            {
                output = new byte[sizeUnpacked];
                if (sizeSheared < sizeUnpacked)
                {
                    Array.Copy(buffer, 0, output, 0, sizeSheared);
                }
                else
                {
                    Array.Copy(buffer, 0, output, 0, sizeUnpacked);
                }
            }

            return output;
        }

        private byte[] Inflate(byte[] buffer, int sizeDecompressed, out int sizeCompressed, int compressionLevel)
        {

            MemoryStream output = new MemoryStream();
            Ionic.Zlib.ZlibStream zs = new Ionic.Zlib.ZlibStream(output, Ionic.Zlib.CompressionMode.Compress, (Ionic.Zlib.CompressionLevel)compressionLevel, true);
            zs.Write(buffer, 0, sizeDecompressed);
            zs.Flush();
            zs.Close();
            sizeCompressed = (int)output.Length;
            return output.ToArray();
        }

        private byte[] Encrypt(byte[] buffer, int size, out int sizePadded)
        {
            int AES_BLOCK_SIZE = AES_KEY.Length;
            sizePadded = size + (AES_BLOCK_SIZE - (size % AES_BLOCK_SIZE));
            byte[] output = new byte[sizePadded];
            byte[] temp = new byte[sizePadded];
            Array.Copy(buffer, 0, temp, 0, buffer.Length);
            buffer = null;
            Rijndael aes = Rijndael.Create();
            aes.Mode = CipherMode.ECB;

            ICryptoTransform encrypt = aes.CreateEncryptor(Encoding.ASCII.GetBytes(AES_KEY), new byte[16]);
            encrypt.TransformBlock(temp, 0, sizePadded, output, 0);
            temp = null;
            return output;
        }

        private byte[] Pack(byte[] buffer, int sizeUnpacked, out int sizeSheared, out int sizeStored, bool encrypt, bool compress, int compressionLevel)
        {
            byte[] output = buffer;
            buffer = null;
            sizeSheared = sizeUnpacked;
            sizeStored = sizeSheared;

            if (compress)
            {
                byte[] tmp = Inflate(output, sizeUnpacked, out sizeSheared, compressionLevel);
                sizeStored = sizeSheared;
                output = tmp;
                tmp = null;
            }

            if (encrypt)
            {
                byte[] tmp = Encrypt(output, sizeSheared, out sizeStored);
                output = tmp;
                tmp = null;
            }
            return output;
        }

        public void Extract(string FileName, bool is64 = false)
        {
            FileStream fs = new FileStream(FileName, FileMode.Open);
            BinaryReader br = new BinaryReader(fs);
            string file_path;
            byte[] buffer_packed;
            byte[] buffer_unpacked;

            byte[] Signature = br.ReadBytes(8);
            uint Version = br.ReadUInt32();

            byte[] Unknown_001 = br.ReadBytes(5);
            int FileDataSizePacked = is64 ? (int)br.ReadInt64() : br.ReadInt32();
            int FileCount = is64 ? (int)br.ReadInt64() : br.ReadInt32();
            bool IsCompressed = br.ReadByte() == 1;
            bool IsEncrypted = br.ReadByte() == 1;
            byte[] Unknown_002 = br.ReadBytes(62);
            int FileTableSizePacked = is64 ? (int)br.ReadInt64() : br.ReadInt32();
            int FileTableSizeUnpacked = is64 ? (int)br.ReadInt64() : br.ReadInt32();

            buffer_packed = br.ReadBytes(FileTableSizePacked);
            int OffsetGlobal = is64 ? (int)br.ReadInt64() : br.ReadInt32();
            OffsetGlobal = (int)br.BaseStream.Position; // don't trust value, read current stream location.

            byte[] FileTableUnpacked = Unpack(buffer_packed, FileTableSizePacked, FileTableSizePacked, FileTableSizeUnpacked, IsEncrypted, IsCompressed);
            buffer_packed = null;
            MemoryStream ms = new MemoryStream(FileTableUnpacked);
            BinaryReader br2 = new BinaryReader(ms);

            for (int i = 0; i < FileCount; i++)
            {
                BPKG_FTE FileTableEntry = new BPKG_FTE();
                FileTableEntry.FilePathLength = is64 ? (int)br2.ReadInt64() : br2.ReadInt32();
                FileTableEntry.FilePath = Encoding.Unicode.GetString(br2.ReadBytes(FileTableEntry.FilePathLength * 2));
                FileTableEntry.Unknown_001 = br2.ReadByte();
                FileTableEntry.IsCompressed = br2.ReadByte() == 1;
                FileTableEntry.IsEncrypted = br2.ReadByte() == 1;
                FileTableEntry.Unknown_002 = br2.ReadByte();
                FileTableEntry.FileDataSizeUnpacked = is64 ? (int)br2.ReadInt64() : br2.ReadInt32();
                FileTableEntry.FileDataSizeSheared = is64 ? (int)br2.ReadInt64() : br2.ReadInt32();
                FileTableEntry.FileDataSizeStored = is64 ? (int)br2.ReadInt64() : br2.ReadInt32();
                FileTableEntry.FileDataOffset = (is64 ? (int)br2.ReadInt64() : br2.ReadInt32()) + OffsetGlobal;
                FileTableEntry.Padding = br2.ReadBytes(60);

                file_path = String.Format("{0}.files\\{1}", FileName, FileTableEntry.FilePath);
                if (!Directory.Exists((new FileInfo(file_path)).DirectoryName))
                    Directory.CreateDirectory((new FileInfo(file_path)).DirectoryName);

                br.BaseStream.Position = FileTableEntry.FileDataOffset;
                buffer_packed = br.ReadBytes(FileTableEntry.FileDataSizeStored);
                buffer_unpacked = Unpack(buffer_packed, FileTableEntry.FileDataSizeStored, FileTableEntry.FileDataSizeSheared, FileTableEntry.FileDataSizeUnpacked, FileTableEntry.IsEncrypted, FileTableEntry.IsCompressed);
                buffer_packed = null;
                FileTableEntry = null;

                if (file_path.EndsWith("xml") || file_path.EndsWith("x16"))
                {
                    // decode bxml
                    MemoryStream temp = new MemoryStream();
                    MemoryStream temp2 = new MemoryStream(buffer_unpacked);
                    BXML bns_xml = new BXML(XOR_KEY);
                    Convert(temp2, bns_xml.DetectType(temp2), temp, BXML_TYPE.BXML_PLAIN);
                    temp2.Close();
                    File.WriteAllBytes(file_path, temp.ToArray());
                    temp.Close();
                    buffer_unpacked = null;
                }
                else
                {
                    // extract raw
                    File.WriteAllBytes(file_path, buffer_unpacked);
                    buffer_unpacked = null;
                }

                // Report progress                                
                int whattosend = (100 * i / 2 / FileCount);

                BNS_Tools.MainWindow.currentMainWindow.SortOutputHandler(whattosend);
                
                // End report progress
            }

            // Report job done
            //Revamped_BnS_Buddy.Form1.CurrentForm.SortOutputHandler("Done!");
            // End report

            br2.Close();
            ms.Close();
            br2 = null;
            ms = null;
            br.Close();
            fs.Close();
            br = null;
            fs = null;
        }

        public void Compress(string Folder, bool is64 = false, int compression = 9)
        {
            string file_path;
            byte[] buffer_packed;
            byte[] buffer_unpacked;

            string[] files = Directory.EnumerateFiles(Folder, "*", SearchOption.AllDirectories).ToArray();

            int FileCount = files.Count();

            BPKG_FTE FileTableEntry = new BPKG_FTE();
            MemoryStream mosTablems = new MemoryStream();
            BinaryWriter mosTable = new BinaryWriter(mosTablems);
            MemoryStream mosFilesms = new MemoryStream();
            BinaryWriter mosFiles = new BinaryWriter(mosFilesms);

            for (int i = 0; i < FileCount; i++)
            {
                file_path = files[i].Replace(Folder, "").TrimStart('\\');
                FileTableEntry.FilePathLength = file_path.Length;

                if (is64)
                    mosTable.Write((long)FileTableEntry.FilePathLength);
                else
                    mosTable.Write(FileTableEntry.FilePathLength);

                FileTableEntry.FilePath = file_path;
                mosTable.Write(Encoding.Unicode.GetBytes(FileTableEntry.FilePath));
                FileTableEntry.Unknown_001 = 2;
                mosTable.Write(FileTableEntry.Unknown_001);
                FileTableEntry.IsCompressed = true;
                mosTable.Write(FileTableEntry.IsCompressed);
                FileTableEntry.IsEncrypted = true;
                mosTable.Write(FileTableEntry.IsEncrypted);
                FileTableEntry.Unknown_002 = 0;
                mosTable.Write(FileTableEntry.Unknown_002);

                FileStream fis = new FileStream(files[i], FileMode.Open);
                MemoryStream tmp = new MemoryStream();

                if (file_path.EndsWith(".xml") || file_path.EndsWith(".x16"))
                {
                    // encode bxml
                    BXML bxml = new BXML(XOR_KEY);
                    Convert(fis, bxml.DetectType(fis), tmp, BXML_TYPE.BXML_BINARY);
                }
                else
                {
                    // compress raw
                    fis.CopyTo(tmp);
                }
                fis.Close();
                fis = null;

                FileTableEntry.FileDataOffset = (int)mosFiles.BaseStream.Position;
                FileTableEntry.FileDataSizeUnpacked = (int)tmp.Length;

                if (is64)
                    mosTable.Write((long)FileTableEntry.FileDataSizeUnpacked);
                else
                    mosTable.Write(FileTableEntry.FileDataSizeUnpacked);

                buffer_unpacked = tmp.ToArray();
                tmp.Close();
                tmp = null;
                buffer_packed = Pack(buffer_unpacked, FileTableEntry.FileDataSizeUnpacked, out FileTableEntry.FileDataSizeSheared, out FileTableEntry.FileDataSizeStored, FileTableEntry.IsEncrypted, FileTableEntry.IsCompressed, compression);
                buffer_unpacked = null;
                mosFiles.Write(buffer_packed);
                buffer_packed = null;

                if (is64)
                    mosTable.Write((long)FileTableEntry.FileDataSizeSheared);
                else
                    mosTable.Write(FileTableEntry.FileDataSizeSheared);

                if (is64)
                    mosTable.Write((long)FileTableEntry.FileDataSizeStored);
                else
                    mosTable.Write(FileTableEntry.FileDataSizeStored);

                if (is64)
                    mosTable.Write((long)FileTableEntry.FileDataOffset);
                else
                    mosTable.Write(FileTableEntry.FileDataOffset);

                FileTableEntry.Padding = new byte[60];
                mosTable.Write(FileTableEntry.Padding);

                // Report progress                
                int whattosend = (100 * (i + FileCount) / 2 / FileCount);

                BNS_Tools.MainWindow.currentMainWindow.SortOutputHandler(whattosend);
                // End report progress
            }

            // Report job done
            //Revamped_BnS_Buddy.Form1.CurrentForm.SortOutputHandler("Packing!");
            // End report

            MemoryStream output = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(output);
            byte[] Signature = new byte[8] { (byte)'U', (byte)'O', (byte)'S', (byte)'E', (byte)'D', (byte)'A', (byte)'L', (byte)'B' };
            bw.Write(Signature);
            int Version = 2;
            bw.Write(Version);
            byte[] Unknown_001 = new byte[5] { 0, 0, 0, 0, 0 };
            bw.Write(Unknown_001);
            int FileDataSizePacked = (int)mosFiles.BaseStream.Length;

            if (is64)
            {
                bw.Write((long)FileDataSizePacked);
                bw.Write((long)FileCount);
            }
            else
            {
                bw.Write(FileDataSizePacked);
                bw.Write(FileCount);
            }

            bool IsCompressed = true;
            bw.Write(IsCompressed);
            bool IsEncrypted = true;
            bw.Write(IsEncrypted);
            byte[] Unknown_002 = new byte[62];
            bw.Write(Unknown_002);

            int FileTableSizeUnpacked = (int)mosTable.BaseStream.Length;
            int FileTableSizeSheared = FileTableSizeUnpacked;
            int FileTableSizePacked = FileTableSizeUnpacked;
            buffer_unpacked = mosTablems.ToArray();
            mosTable.Close();
            mosTablems.Close();
            mosTable = null;
            mosTablems = null;
            buffer_packed = Pack(buffer_unpacked, FileTableSizeUnpacked, out FileTableSizeSheared, out FileTableSizePacked, IsEncrypted, IsCompressed, compression);
            buffer_unpacked = null;

            if (is64)
                bw.Write((long)FileTableSizePacked);
            else
                bw.Write(FileTableSizePacked);

            if (is64)
                bw.Write((long)FileTableSizeUnpacked);
            else
                bw.Write(FileTableSizeUnpacked);

            bw.Write(buffer_packed);
            buffer_packed = null;

            int OffsetGlobal = (int)output.Position + (is64 ? 8 : 4);

            if (is64)
                bw.Write((long)OffsetGlobal);
            else
                bw.Write(OffsetGlobal);

            buffer_packed = mosFilesms.ToArray();
            mosFiles.Close();
            mosFilesms.Close();
            mosFiles = null;
            mosFilesms = null;
            bw.Write(buffer_packed);
            buffer_packed = null;
            File.WriteAllBytes(Folder.Replace(".files", ""), output.ToArray());
            bw.Close();
            output.Close();
            bw = null;
            output = null;

            // Report job done
            //Revamped_BnS_Buddy.Form1.CurrentForm.SortOutputHandler("Done!");
            // End report
        }

        private void Convert(Stream iStream, BXML_TYPE iType, Stream oStream, BXML_TYPE oType)
        {
            if ((iType == BXML_TYPE.BXML_PLAIN && oType == BXML_TYPE.BXML_BINARY) || (iType == BXML_TYPE.BXML_BINARY && oType == BXML_TYPE.BXML_PLAIN))
            {
                BXML bns_xml = new BXML(XOR_KEY);
                bns_xml.Load(iStream, iType);
                bns_xml.Save(oStream, oType);
            }
            else
            {
                iStream.CopyTo(oStream);
            }
        }
    }

    class BXML_CONTENT
    {
        public byte[] XOR_KEY;

        void Xor(byte[] buffer, int size)
        {
            for (int i = 0; i < size; i++)
            {
                buffer[i] = (byte)(buffer[i] ^ XOR_KEY[i % XOR_KEY.Length]);
            }
        }

        bool Keep_XML_WhiteSpace = true;

        public void Read(Stream iStream, BXML_TYPE iType)
        {
            if (iType == BXML_TYPE.BXML_PLAIN)
            {
                Signature = new byte[8] { (byte)'L', (byte)'M', (byte)'X', (byte)'B', (byte)'O', (byte)'S', (byte)'L', (byte)'B' };
                Version = 3;
                FileSize = 85;
                Padding = new byte[64];
                Unknown = true;
                OriginalPathLength = 0;

                // NOTE: keep whitespace text nodes (to be compliant with the whitespace TEXT_NODES in bns xml)
                // no we don't keep them, we remove them because it is cleaner
                Nodes.PreserveWhitespace = Keep_XML_WhiteSpace;
                Nodes.Load(iStream);

                // get original path from first comment node
                XmlNode node = Nodes.DocumentElement.ChildNodes.OfType<XmlComment>().First();
                if (node != null && node.NodeType == XmlNodeType.Comment)
                {
                    string Text = node.InnerText;
                    OriginalPathLength = Text.Length;
                    OriginalPath = Encoding.Unicode.GetBytes(Text);
                    Xor(OriginalPath, 2 * OriginalPathLength);
                    if (Nodes.PreserveWhitespace && node.NextSibling.NodeType == XmlNodeType.Whitespace)
                        Nodes.DocumentElement.RemoveChild(node.NextSibling);
                }
                else
                {
                    OriginalPath = new byte[2 * OriginalPathLength];
                }
            }

            if (iType == BXML_TYPE.BXML_BINARY)
            {
                Signature = new byte[8];
                BinaryReader br = new BinaryReader(iStream);
                br.BaseStream.Position = 0;
                Signature = br.ReadBytes(8);
                Version = br.ReadInt32();
                FileSize = br.ReadInt32();
                Padding = br.ReadBytes(64);
                Unknown = br.ReadByte() == 1;
                OriginalPathLength = br.ReadInt32();
                OriginalPath = br.ReadBytes(2 * OriginalPathLength);
                AutoID = 1;
                ReadNode(iStream);

                // add original path as first comment node
                byte[] Path = OriginalPath;
                Xor(Path, 2 * OriginalPathLength);
                XmlComment node = Nodes.CreateComment(Encoding.Unicode.GetString(Path));
                Nodes.DocumentElement.PrependChild(node);
                XmlNode docNode = Nodes.CreateXmlDeclaration("1.0", "utf-8", null);
                Nodes.PrependChild(docNode);
                if (FileSize != iStream.Position)
                {
                    throw new Exception(String.Format("Filesize Mismatch, expected size was {0} while actual size was {1}.", FileSize, iStream.Position));
                }
            }
        }

        public void Write(Stream oStream, BXML_TYPE oType)
        {
            if (oType == BXML_TYPE.BXML_PLAIN)
            {
                Nodes.Save(oStream);
            }
            if (oType == BXML_TYPE.BXML_BINARY)
            {
                BinaryWriter bw = new BinaryWriter(oStream);
                bw.Write(Signature);
                bw.Write(Version);
                bw.Write(FileSize);
                bw.Write(Padding);
                bw.Write(Unknown);
                bw.Write(OriginalPathLength);
                bw.Write(OriginalPath);

                AutoID = 1;
                WriteNode(oStream);

                FileSize = (int)oStream.Position;
                oStream.Position = 12;
                bw.Write(FileSize);
            }

        }

        private void ReadNode(Stream iStream, XmlNode parent = null)
        {

            XmlNode node = null;
            BinaryReader br = new BinaryReader(iStream);
            int Type = 1;
            if (parent != null)
            {
                Type = br.ReadInt32();
            }


            if (Type == 1)
            {
                node = Nodes.CreateElement("Text");

                int ParameterCount = br.ReadInt32();
                for (int i = 0; i < ParameterCount; i++)
                {
                    int NameLength = br.ReadInt32();
                    byte[] Name = br.ReadBytes(2 * NameLength);
                    Xor(Name, 2 * NameLength);

                    int ValueLength = br.ReadInt32();
                    byte[] Value = br.ReadBytes(2 * ValueLength);
                    Xor(Value, 2 * ValueLength);

                    ((XmlElement)node).SetAttribute(Encoding.Unicode.GetString(Name), Encoding.Unicode.GetString(Value));
                }
            }

            if (Type == 2)
            {
                node = Nodes.CreateTextNode("");

                int TextLength = br.ReadInt32();
                byte[] Text = br.ReadBytes(TextLength * 2);
                Xor(Text, 2 * TextLength);

                ((XmlText)node).Value = Encoding.Unicode.GetString(Text);
            }

            if (Type > 2)
            {
                throw new Exception("Unknown XML Node Type");
            }

            bool Closed = br.ReadByte() == 1;
            int TagLength = br.ReadInt32();
            byte[] Tag = br.ReadBytes(2 * TagLength);
            Xor(Tag, 2 * TagLength);
            if (Type == 1)
            {
                node = RenameNode(node, Encoding.Unicode.GetString(Tag));
            }

            int ChildCount = br.ReadInt32();
            AutoID = br.ReadInt32();
            AutoID++;

            for (int i = 0; i < ChildCount; i++)
            {
                ReadNode(iStream, node);
            }

            if (parent != null)
            {
                if (Keep_XML_WhiteSpace || Type != 2 || !String.IsNullOrWhiteSpace(node.Value))
                {
                    parent.AppendChild(node);
                }
            }
            else
            {
                Nodes.AppendChild(node);
            }
        }

        public static XmlNode RenameNode(XmlNode node, string Name)
        {
            if (node.NodeType == XmlNodeType.Element)
            {
                XmlElement oldElement = (XmlElement)node;
                XmlElement newElement =
                node.OwnerDocument.CreateElement(Name);

                while (oldElement.HasAttributes)
                {
                    newElement.SetAttributeNode(oldElement.RemoveAttributeNode(oldElement.Attributes[0]));
                }

                while (oldElement.HasChildNodes)
                {
                    newElement.AppendChild(oldElement.FirstChild);
                }

                if (oldElement.ParentNode != null)
                {
                    oldElement.ParentNode.ReplaceChild(newElement, oldElement);
                }

                return newElement;
            }
            else
                return node;
        }

        private bool WriteNode(Stream oStream, XmlNode parent = null)
        {
            BinaryWriter bw = new BinaryWriter(oStream);
            XmlNode node = null;

            int Type = 1;
            if (parent != null)
            {
                node = parent;
                if (node.NodeType == XmlNodeType.Element)
                {
                    Type = 1;
                }
                if (node.NodeType == XmlNodeType.Text || node.NodeType == XmlNodeType.Whitespace)
                {
                    Type = 2;
                }
                if (node.NodeType == XmlNodeType.Comment)
                {
                    return false;
                }
                bw.Write(Type);
            }
            else
            {
                node = Nodes.DocumentElement;
            }

            if (Type == 1)
            {
                int OffsetAttributeCount = (int)oStream.Position;
                int AttributeCount = 0;
                bw.Write(AttributeCount);

                foreach (XmlAttribute attribute in node.Attributes)
                {
                    string Name = attribute.Name;
                    int NameLength = Name.Length;
                    bw.Write(NameLength);
                    byte[] NameBuffer = Encoding.Unicode.GetBytes(Name);
                    Xor(NameBuffer, 2 * NameLength);
                    bw.Write(NameBuffer);

                    String Value = attribute.Value;
                    int ValueLength = Value.Length;
                    bw.Write(ValueLength);
                    byte[] ValueBuffer = Encoding.Unicode.GetBytes(Value);
                    Xor(ValueBuffer, 2 * ValueLength);
                    bw.Write(ValueBuffer);
                    AttributeCount++;
                }

                int OffsetCurrent = (int)oStream.Position;
                oStream.Position = OffsetAttributeCount;
                bw.Write(AttributeCount);
                oStream.Position = OffsetCurrent;
            }

            if (Type == 2)
            {
                string Text = node.Value;
                int TextLength = Text.Length;
                bw.Write(TextLength);
                byte[] TextBuffer = Encoding.Unicode.GetBytes(Text);
                Xor(TextBuffer, 2 * TextLength);
                bw.Write(TextBuffer);

            }

            if (Type > 2)
            {
                throw new Exception(String.Format("ERROR: XML NODE TYPE [{0}] UNKNOWN", node.NodeType.ToString()));
            }

            bool Closed = true;
            bw.Write(Closed);
            string Tag = node.Name;
            int TagLength = Tag.Length;
            bw.Write(TagLength);
            byte[] TagBuffer = Encoding.Unicode.GetBytes(Tag);
            Xor(TagBuffer, 2 * TagLength);
            bw.Write(TagBuffer);

            int OffsetChildCount = (int)oStream.Position;
            int ChildCount = 0;
            bw.Write(ChildCount);
            bw.Write(AutoID);
            AutoID++;

            foreach (XmlNode child in node.ChildNodes)
            {
                if (WriteNode(oStream, child))
                {
                    ChildCount++;
                }
            }

            int OffsetCurrent2 = (int)oStream.Position;
            oStream.Position = OffsetChildCount;
            bw.Write(ChildCount);
            oStream.Position = OffsetCurrent2;
            return true;
        }

        byte[] Signature;                  // 8 byte
        int Version;                   // 4 byte
        int FileSize;                  // 4 byte
        byte[] Padding;                    // 64 byte
        bool Unknown;                       // 1 byte
        // TODO: add to CDATA ?
        int OriginalPathLength;        // 4 byte
        byte[] OriginalPath;               // 2*OriginalPathLength bytes

        int AutoID;
        XmlDocument Nodes = new XmlDocument();
    }

    class BXML
    {
        private BXML_CONTENT _content = new BXML_CONTENT();

        private byte[] XOR_KEY { get { return _content.XOR_KEY; } set { _content.XOR_KEY = value; } }

        public BXML(byte[] xor)
        {
            XOR_KEY = xor;
        }

        public void Load(Stream iStream, BXML_TYPE iType)
        {
            _content.Read(iStream, iType);
        }

        public void Save(Stream oStream, BXML_TYPE oType)
        {
            _content.Write(oStream, oType);
        }

        public BXML_TYPE DetectType(Stream iStream)
        {
            int offset = (int)iStream.Position;
            iStream.Position = 0;
            byte[] Signature = new byte[13];
            iStream.Read(Signature, 0, 13);
            iStream.Position = offset;

            BXML_TYPE result = BXML_TYPE.BXML_UNKNOWN;

            if (
                BitConverter.ToString(Signature).Replace("-", "").Replace("00", "").Contains(BitConverter.ToString(new byte[] { (byte)'<', (byte)'?', (byte)'x', (byte)'m', (byte)'l' }).Replace("-", ""))
            )
            {
                result = BXML_TYPE.BXML_PLAIN;
            }

            if (
                Signature[7] == 'B' &&
                Signature[6] == 'L' &&
                Signature[5] == 'S' &&
                Signature[4] == 'O' &&
                Signature[3] == 'B' &&
                Signature[2] == 'X' &&
                Signature[1] == 'M' &&
                Signature[0] == 'L'
            )
            {
                result = BXML_TYPE.BXML_BINARY;
            }

            return result;
        }
    }
}