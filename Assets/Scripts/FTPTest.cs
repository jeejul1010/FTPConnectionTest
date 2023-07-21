using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.IO;
using System.Text;
using System;
using UnityEngine.UI;
using Newtonsoft.Json;

public class FTPTest : MonoBehaviour
{
    public string m_UserName = "jeejul1010";
    public string m_Password = "E!qeL+5S";

    public string ftpServerAddress = "ftps://nas.uvrlab.org:2121";

    public Dropdown dropdown;
    public Text nameText, livesText, healthText;

    void Awake()
    {
        ftpServerAddress = "ftps://nas.uvrlab.org:2121";
        FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpServerAddress);
        request.Method = WebRequestMethods.Ftp.ListDirectory;  

        request.Credentials = new NetworkCredential(m_UserName, m_Password); 
        request.EnableSsl = true; // TLS/SSL

        var cert = new ForceAcceptAll();
        // request.certi = cert;

        // request.UseBinary = true;
        // request.Proxy = null;
        // request.UsePassive = true;
        FtpWebResponse response = (FtpWebResponse)request.GetResponse();  
        Stream responseStream = response.GetResponseStream();  
        StreamReader reader = new StreamReader(responseStream);  
        string names = reader.ReadToEnd();  

        reader.Close();  
        response.Close();

        string[] fileList = names.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        
        dropdown.ClearOptions();
        foreach(var item in fileList)
        {
            var ext = Path.GetExtension(item);
            if(ext != ".Json") continue;
            var newItem = Path.GetFileNameWithoutExtension(item);
            var option = new Dropdown.OptionData(newItem);
            dropdown.options.Add(option);
        }
    }

    public void FTPUpload()
    {
        PlayerState myPlayerState = new PlayerState();

        myPlayerState.playerName = "RandomPlayer";
        myPlayerState.lives = UnityEngine.Random.Range(1, 10);
        myPlayerState.health = UnityEngine.Random.Range(1.0f, 100.0f);
        myPlayerState.levelCleared = new bool[8] {true, false, false, false, false, false, false, false};

        string json = JsonUtility.ToJson(myPlayerState);

        string fileName = "test_" + DateTime.Now.ToString("HHmmss");
        string path = Application.streamingAssetsPath + "/" + fileName + ".Json";

        FileStream fileStream = new FileStream(path, FileMode.Create);
        byte[] data = Encoding.UTF8.GetBytes(json);
        fileStream.Write(data, 0, data.Length);
        fileStream.Close();

        FtpWebRequest ftpWebRequest = (FtpWebRequest)WebRequest.Create(ftpServerAddress + fileName +".Json");
        ftpWebRequest.Credentials = new NetworkCredential(m_UserName, m_Password);
        ftpWebRequest.EnableSsl = true; // TLS/SSL
        ftpWebRequest.UseBinary = false;   // ASCII, Binary(디폴트)
        ftpWebRequest.Method = WebRequestMethods.Ftp.UploadFile;
                
        byte[] ftpdata = File.ReadAllBytes(path);

        using (var ftpStream = ftpWebRequest.GetRequestStream())
        {
            ftpStream.Write(ftpdata, 0, ftpdata.Length);
        }
                
        using (var responses = (FtpWebResponse)ftpWebRequest.GetResponse())
        {
            Debug.Log(responses.StatusDescription);
        }

        FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpServerAddress);  
        request.Method = WebRequestMethods.Ftp.ListDirectory;  

        request.Credentials = new NetworkCredential(m_UserName, m_Password);
        ftpWebRequest.EnableSsl = true; // TLS/SSL 
        FtpWebResponse response = (FtpWebResponse)request.GetResponse();  
        Stream responseStream = response.GetResponseStream();  
        StreamReader reader = new StreamReader(responseStream);  
        string names = reader.ReadToEnd();  

        reader.Close();  
        response.Close();

        string[] fileList = names.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        
        dropdown.ClearOptions();
        foreach(var item in fileList)
        {
            var ext = Path.GetExtension(item);
            if(ext != ".Json") continue;
            var newItem = Path.GetFileNameWithoutExtension(item);
            var option = new Dropdown.OptionData(newItem);
            dropdown.options.Add(option);
        }


    }

    public void FTPDownload()
    {
        string fileName = dropdown.options[dropdown.value].text;
        string path = Application.streamingAssetsPath + "/" + fileName + ".Json";

        FtpWebRequest ftpWebRequest = (FtpWebRequest)WebRequest.Create(ftpServerAddress + fileName +".Json");
        ftpWebRequest.Credentials = new NetworkCredential(m_UserName, m_Password);
        ftpWebRequest.EnableSsl = true; // TLS/SSL
        ftpWebRequest.Method = WebRequestMethods.Ftp.DownloadFile;

        using (var localfile = File.Open(path, FileMode.Create))
        using (var ftpStream = ftpWebRequest.GetResponse().GetResponseStream())
        {
            byte[] buffer = new byte[1024];
            int n;
            while ((n = ftpStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                localfile.Write(buffer, 0, n);
            }
        }

        using (StreamReader r = new StreamReader(path))
        {
            string json = r.ReadToEnd();
            PlayerState items = JsonConvert.DeserializeObject<PlayerState>(json);
            nameText.text = "PlayerName: " + items.playerName;
            livesText.text = "Lives: " + items.lives.ToString();
            healthText.text = "Health: " + items.health.ToString();
        }
        
    }

    public void DropdownUpdate()
    {
        FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpServerAddress);  
        request.Method = WebRequestMethods.Ftp.ListDirectory;  

        request.Credentials = new NetworkCredential(m_UserName, m_Password);
        request.EnableSsl = true; // TLS/SSL
        FtpWebResponse response = (FtpWebResponse)request.GetResponse();  
        Stream responseStream = response.GetResponseStream();  
        StreamReader reader = new StreamReader(responseStream);  
        string names = reader.ReadToEnd();  

        reader.Close();  
        response.Close();

        string[] fileList = names.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        
        dropdown.ClearOptions();
        foreach(var item in fileList)
        {
            var ext = Path.GetExtension(item);
            if(ext != ".Json") continue;
            var newItem = Path.GetFileNameWithoutExtension(item);
            var option = new Dropdown.OptionData(newItem);
            dropdown.options.Add(option);
        }
        
    }
}
