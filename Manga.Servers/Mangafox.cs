using HtmlAgilityPack;
using Manga.Framework;
using Manga.Interfaces;
using Manga.Structures.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Manga.Servers
{
    public class Mangafox : ObservableObject, IServer
    {
        #region Members

        private bool _isRunning;
        System.Text.RegularExpressions.Regex regex;
        string sizepattern = "(?<=of )([0-9]*)(?=\\t)";
        
        #endregion

        #region Semaphores
        /// TODO

        public readonly object semRunningRead;
        public readonly object semRunningWrite;

        #endregion

        #region Properties
        /// <summary>
        /// State of Task
        /// </summary>
        public bool IsRunning
        {
            get
            {
                return _isRunning;
            }
            set
            {
                _isRunning = value;
                RaisePropertyChanged("IsRunning");
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes and compiles a new instace of Manga.Servers.Mangafox object class.
        /// </summary>
        public Mangafox()
        {
            regex = new System.Text.RegularExpressions.Regex(sizepattern);
        }

        #endregion
        

        #region Common

        /// <summary>
        /// Extract the chapter size from the content of the chapter page content
        /// </summary>
        /// <param name="Link">Link of the chapter</param>
        /// <returns>Number of chapters as 32-Bit integer</returns>
        protected int GetChapterSize(string Link)
        {
            // number to return
            int value;
            // skipping this part for more info check other parts
            HtmlDocument Doc = new HtmlDocument();
            System.Net.WebRequest webReq = System.Net.WebRequest.Create(Link);
            using (System.Net.WebResponse webRes = webReq.GetResponse())
            {
                using (System.IO.Stream mystream = webRes.GetResponseStream())
                {
                    if (mystream != null)
                    {
                        Doc.Load(mystream);
                    }
                    else
                    {
                        webRes.Close();
                        return -1;
                    }

                    HtmlNode node = Doc.DocumentNode.SelectSingleNode("//div[@class='l']");
                    // get the size part from html content by using regular expressions
                    value = int.Parse(regex.Match(node.InnerText).Value);

                    mystream.Close();
                }
                webRes.Close();
            }
            return value;
        }

        #endregion

        #region Info - Info Window

        public void GetInfoExtended(object obj, bool State)
        {
            MangaModel model = obj as MangaModel;

            try
            {
                HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(model.Link);
                using (HttpWebResponse webRes = (HttpWebResponse)webReq.GetResponse())
                {
                    using (System.IO.StreamReader mystream = new StreamReader(webRes.GetResponseStream()))
                    {
                        if (State)
                            return;

                        HtmlDocument doc = new HtmlDocument();

                        if (mystream != null)
                        {
                            doc.Load(mystream);
                        }

                        if (State)
                            return;

                        HtmlNodeCollection col = doc.DocumentNode.SelectNodes("//*[@id='title']//table//tr[2]/td");

                        model.Year = col[0].SelectSingleNode("a").InnerText;

                        if (State)
                            return;

                        HtmlNodeCollection authcol = col[1].SelectNodes("a");

                        if (authcol != null)
                        {
                            model.Author = null;
                            foreach (HtmlNode node in authcol)
                            {
                                model.Author += node.InnerText;
                            }
                        }

                        if (State)
                            return;

                        HtmlNodeCollection artcol = col[2].SelectNodes("a");

                        if (artcol != null)
                        {
                            model.Artist = null;
                            foreach (HtmlNode node in artcol)
                            {
                                model.Artist += node.InnerText;
                            }
                        }

                        if (State)
                            return;

                        HtmlNodeCollection genrecol = col[3].SelectNodes("a");

                        if (genrecol != null)
                        {
                            model.Genres = new string[genrecol.Count];
                            for (int i = 0; i < genrecol.Count; i++)
                            {
                                model.Genres[i] = genrecol[i].InnerText;
                            }
                        }

                        if (State)
                            return;

                        HtmlNode node2 = doc.DocumentNode.SelectSingleNode("//*[@id='title']//p[@class='summary']");
                        String summary = (node2 != null ? node2.InnerText : null);
                        model.Description = summary;

                        if (State)
                            return;

                        HtmlNode imgnode = doc.DocumentNode.SelectSingleNode("//div[@class='left']//div[@id='series_info']//div[@class='cover']//img");
                        if (imgnode != null)
                        {
                            string imgpath = imgnode.Attributes["src"].Value;

                            //manga.CoverImage = GetImageFromUrl(imgpath);
                            model.ImageSource = new Uri(imgpath);
                        }

                        if (State)
                            return;

                        HtmlNodeCollection coll = doc.DocumentNode.SelectNodes("//div[@id='chapters']//ul[@class='chlist']//li//div");
                        if (coll != null)
                        {
                            List<ChapterModel> tempList = new List<ChapterModel>(col.Count);
                            for (int i = coll.Count - 1; i >= 0; i--)
                            {
                                if (State)
                                    break;

                                ChapterModel chptr = new ChapterModel();
                                String name1;
                                HtmlNode node1 = coll[i].SelectSingleNode("h3//span[@class='title nowrap'] | h4//span[@class='title nowrap']");
                                if (node1 == null)
                                    name1 = null;
                                else
                                    name1 = coll[i].SelectSingleNode("h3//span[@class='title nowrap'] | h4//span[@class='title nowrap']").InnerText;
                                String name2 = coll[i].SelectSingleNode("h3//a | h4//a").InnerText.RemoveExtaSpaces();
                                chptr.Name = (name1 == null ? name2.RemoveExtaSpaces() : (name2.RemoveExtaSpaces() + " : " + name1.RemoveExtaSpaces()));
                                chptr.Link = coll[i].SelectSingleNode("h3//a | h4//a").Attributes["href"].Value;
                                chptr.Id = model.Id;
                                chptr.Date = coll[i].SelectSingleNode("span[@class='date']").InnerText;
                                chptr.Parent = model;

                                if (model.Items.Contains(chptr) == true)
                                    continue;
                                tempList.Add(chptr);
                            }
                            model.Items.AddRange(tempList);
                            model.Count = model.Items.Count;
                        }

                        //db.UpdateManga(manga);
                    }
                }
                model.InfoState = 2;
            }
            catch (WebException)
            {
                //errorCallback.Invoke("Internet Connection Error, Check your connection");
            }
            catch (Exception)
            {
                //errorCallback.Invoke(e.Message);
            }
        }

        #endregion

        #region Info - Download

        public void GetInfoMinimal(IMangaModel model, bool State)
        {
            // cast mangamodel parameter
            // create another instance of server which allows us to get info from seperate thread
            model.Publisher = Publisher.Mangafox;

            try
            {
                // create a web request to the manga homepage
                HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(model.Link);
                using (HttpWebResponse webRes = (HttpWebResponse)webReq.GetResponse()) // get web response
                {
                    // open a streamreader to read response content
                    using (System.IO.StreamReader mystream = new StreamReader(webRes.GetResponseStream()))
                    {
                        // htmldocument is used for seeing html code as xml code and selecting spesific parts easily
                        // it decreases time to extract the parts we want from html
                        HtmlDocument doc = new HtmlDocument();
                        // if there is a stream load the html content
                        if (mystream != null)
                        {
                            doc.Load(mystream);
                        }

                        // create a collection of nodes which holds chapters division in html code
                        HtmlNodeCollection coll = doc.DocumentNode.SelectNodes("//div[@id='chapters']//ul[@class='chlist']//li//div");
                        if (coll != null) // if there are chapters which means collection is not empty, continue
                        {
                            // foreach node in collection, get info about chapter such as
                            for (int i = coll.Count - 1; i >= 0; i--)
                            {
                                ChapterModel chptr = new ChapterModel();
                                String name1;
                                // get the extended name of the chapter such as Chapter 13 - !""This part""!
                                HtmlNode node1 = coll[i].SelectSingleNode("h3//span[@class='title nowrap'] | h4//span[@class='title nowrap']");
                                if (node1 == null) // if extended name doesnt exists set name1 to null
                                    name1 = null;
                                else // otherwise get the name and store it in name1 field
                                    name1 = coll[i].SelectSingleNode("h3//span[@class='title nowrap'] | h4//span[@class='title nowrap']").InnerText;
                                // get the number of  the chapter such as !""This Part""! - Hello World and store it in name2 field
                                String name2 = coll[i].SelectSingleNode("h3//a | h4//a").InnerText.RemoveExtaSpaces();
                                // merge the name, if name1 exsists, store them in one string-name-, if it does not store only name2
                                chptr.Name = (name1 == null ? name2.RemoveExtaSpaces() : (name2.RemoveExtaSpaces() + " : " + name1.RemoveExtaSpaces()));
                                // get link of the chapter from nodes
                                chptr.Link = coll[i].SelectSingleNode("h3//a | h4//a").Attributes["href"].Value;
                                // get unique id of the manga and set it to this chapter                                
                                chptr.Id = model.Id;
                                // get release date of the chapter
                                // TODO
                                // convert 'today', 'yesterday', and these things to the date later
                                chptr.Date = coll[i].SelectSingleNode("span[@class='date']").InnerText;
                                // set parent of the chapter to the manga
                                chptr.Parent = model;
                                // if there is a chapter such as this one dont add
                                if (model.Items.Contains(chptr) == true)
                                    continue;
                                // add chapter to the model
                                model.Items.Add(chptr);
                            }
                            model.Count = model.Items.Count;
                        }
                        model.InfoState = 1;
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        #endregion
        
        #region Download

        /// <summary>
        /// Download the manga
        /// </summary>
        /// <param name="obj">Manga to download</param>
        /// <param name="State">State of the task</param>
        public void Download(object obj, bool State)
        {
            DownloadModel model = obj as DownloadModel;

            if (model.Items.Count > 0)
            {
                model.ChaptersProgress = 0;
                model.ChaptersSize = model.Items.Count;

                for (int i = 0; i < model.Items.Count; i++)
                {
                    model.Items[i].Size = GetChapterSize(model.Items[i].Link);
                    model.ImagesProgress = 0;
                    model.ImagesSize = model.Items[i].Size;
                    DownloadChapterToFolder(model, model.Items[i], ref State);
                    model.ChaptersProgress++;
                }
            }
        }

        protected void DownloadChapterToFolder(DownloadModel model, DownloadItem item, ref bool state)
        {
            string dir = string.Format("{0}\\{1}\\{2}", Manga.Structures.Properties.Settings.Default.DownloadPath,
                    model.Name.Trim().RemoveIllegalCharacters(),
                    item.Name.Trim().RemoveIllegalCharacters());

            System.IO.Directory.CreateDirectory(dir);

            PriorityQueue<DownloadFile, Priority> _queue = null;

            if (item.Size > 0)
            {
                _queue = new PriorityQueue<DownloadFile, Priority>() { Priority = Framework.Priority.Normal };
                _queue.Size = item.Size;                
                ParallelWorker.InstanceDownload.EnqueueFileToDownload(_queue);
            }

            for (int i = 1; i <= item.Size; i++)
            {
                if (state)
                    break;

                HtmlDocument doc = new HtmlDocument();
                int j = item.Link.LastIndexOf('/');
                string link = string.Format("{0}{1}.html", item.Link.Substring(0, j + 1), i);
                string imagelink;

                System.Net.WebRequest webReq = System.Net.WebRequest.Create(link);
                using (System.Net.WebResponse webRes = webReq.GetResponse())
                {
                    using (System.IO.Stream mystream = webRes.GetResponseStream())
                    {
                        if (mystream != null)
                        {
                            doc.Load(mystream);
                        }

                        HtmlNode node = doc.DocumentNode.SelectSingleNode("//body/div/a/img");
                        imagelink = node.Attributes["src"].Value;
                        mystream.Close();
                    }
                    webRes.Close();
                }

                DownloadFile file = new DownloadFile();
                file.PathToSave = string.Format("{0}\\{1}.jpg", dir, i);
                file.Index = i;
                file.Url = imagelink;
                file.FinishedEvent += new DownloadFinishedEventHandler((sender) => { DownloadFinished(sender, model); });

                _queue.Enqueue(file, Priority.Normal);
            }
            // Wait for parallel worker to download the whole chapter
            ParallelWorker.InstanceDownload.WaitAll();
        }

        private void DownloadFinished(DownloadFile sender, DownloadModel model)
        {
            model.ImagesProgress++;
        }

        #endregion

        #region Fetch List

        /// <summary>
        /// Get the list from mangafox directory
        /// </summary>
        /// <param name="obj">List to populate</param>
        /// <param name="State">State of the task</param>
        public void FetchList(object obj, System.Threading.CancellationToken Token)
        {
            // cast first parameter to threadsafeobservablecollection
            ThreadSafeObservableCollection<MangaModel> model = obj as ThreadSafeObservableCollection<MangaModel>;

            HttpWebRequest webReq = null;
            HttpWebResponse webRes = null;

            try
            {
                // create a webrequest to manga directory of publisher
                webReq = (HttpWebRequest)WebRequest.Create("http://mangafox.me/manga/");
                webReq.KeepAlive = true; // keep alive to prevent timeout
                using (webRes = (HttpWebResponse)webReq.GetResponse()) // get web response
                {   // open a streamreader to read response content
                    using (System.IO.StreamReader mystream = new StreamReader(webRes.GetResponseStream()))
                    {
                        Token.ThrowIfCancellationRequested();
                        // htmldocument is used for seeing html code as xml code and selecting spesific parts easily
                        // it decreases time to extract the parts we want from html
                        HtmlDocument doc = new HtmlDocument();
                        // if there is a stream load the html content
                        if (mystream != null)
                        {
                            doc.Load(mystream);
                        }

                        // select the manga list div field and get all the links from it
                        HtmlNodeCollection col = doc.DocumentNode.SelectNodes("//div[@class='manga_list']//li//a");
                        // create an array of mangamodel[number of nodes in col]
                        List<MangaModel> items = new List<MangaModel>();
                        // for each nodes in collection create a manga and fill the manga information and add it to the array
                        for (int i = 0; i < col.Count; i++)
                        {
                            Token.ThrowIfCancellationRequested();
                            
                            MangaModel manga = new MangaModel();
                            manga.Name = col[i].InnerText;
                            manga.Link = col[i].Attributes["href"].Value;
                            manga.Publisher = Publisher.Mangafox;
                            if (model.Contains(manga))
                                continue;
                            items.Add(manga);
                        }
                        // if there are manga in nodes collection, add them to our threadsafe observable collection in one function
                        // instead of adding all of the mangas one at a time, it prevents notifying the listeners that a manga
                        // is added everytime, instead of that it supresses the signal at the start of the func, then adds mangas,
                        // after that it notifies collection is changed
                        if (items.Count > 0)
                            model.AddRange(items);
                        // Add to DB


                    }
                }
            }
            catch (OperationCanceledException)
            {
                
            }
            catch (WebException)
            {
                if (webRes != null)
                    webRes.Close();
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region Reader

        public void FindImages(IReaderModel model, bool State)
        {
            for (int i = 0; i < model.Items.Count; i++)
            {
                FindImages(model.Items[i], State);
            }
        }

        private bool CheckChapterFolder(IChapterModel model)
        {
            if (Directory.Exists(model.Path))
            {
                DirectoryInfo info = new DirectoryInfo(model.Path);
                FileInfo[] items = info.GetFiles();

                Array.Sort(items, new FileInfoComparer());

                List<String> files = new List<string>();

                foreach (FileInfo file in items)
                {
                    files.Add(file.FullName);
                }

                if (model.Size == files.Count)
                {
                    for (int i = 0; i < files.Count; i++)
                    {
                        model.Items.Add(new ImageModel() { Link = files[i], LinkType = LinkType.Local });
                    }

                    if (model.Items.Count > 0)
                    {
                        (model.Parent as ReaderModel).ItemsCreated.Invoke();
                        Database.DatabaseEngine.Instance.Update(model);
                        return true;
                    }
                }
            }
            return false;
        }

        public void FindImages(IChapterModel model, bool State)
        {
            string dir = string.Format("{0}\\{1}\\{2}", Manga.Structures.Properties.Settings.Default.DownloadPath,
                    model.Parent.Name.Trim().RemoveIllegalCharacters(),
                    model.Name.Trim().RemoveIllegalCharacters());

            model.Path = dir;            

            System.IO.Directory.CreateDirectory(dir);

            model.Size = GetChapterSize(model.Link);

            PriorityQueue<DownloadFile, Priority> _queue = null;

            if (model.Size > 0)
            {
                if (CheckChapterFolder(model))
                    return;
                _queue = new PriorityQueue<DownloadFile, Priority>() { Priority = Priority.Read };
                _queue.Size = model.Size;
                ParallelWorker.InstanceDownload.EnqueueFileToDownload(_queue);
            }

            Database.DatabaseEngine.Instance.Update(model);

            for (int i = 0; i < model.Size; i++)
            {
                model.Items.Add(new ImageModel());

                HtmlDocument doc = new HtmlDocument();
                int j = model.Link.LastIndexOf('/');
                string link = string.Format("{0}{1}.html", model.Link.Substring(0, j + 1), i + 1);
                string imagelink;

                System.Net.WebRequest webReq = System.Net.WebRequest.Create(link);
                using (System.Net.WebResponse webRes = webReq.GetResponse())
                {
                    using (System.IO.Stream mystream = webRes.GetResponseStream())
                    {
                        if (mystream != null)
                        {
                            doc.Load(mystream);
                        }

                        HtmlNode node = doc.DocumentNode.SelectSingleNode("//body/div/a/img");
                        imagelink = node.Attributes["src"].Value;
                        mystream.Close();
                    }
                    webRes.Close();
                }

                DownloadFile file = new DownloadFile();
                file.PathToSave = string.Format("{0}\\{1}.jpg", dir, i + 1);
                file.Url = imagelink;
                file.Priority = Priority.Read;
                file.Index = i;
                file.FinishedEvent += new DownloadFinishedEventHandler((sender) =>
                {
                    lock (model.Items)
                    {
                        model.Items[sender.Index].Link = sender.PathToSave;
                        model.Items[sender.Index].LinkType = LinkType.Local;
                    }
                    if(sender.Index == 0)
                        (model.Parent as ReaderModel).ItemsCreated.Invoke();
                });

                _queue.Enqueue(file, Priority.Read);
            }
        }

        #endregion

        #region Update

        public void CheckUpdates(IMangaModel model, System.Threading.CancellationToken Token)
        {
            // cast mangamodel parameter
            // create another instance of server which allows us to get info from seperate thread
            model.Publisher = Publisher.Mangafox;

            try
            {
                // create a web request to the manga homepage
                HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(model.Link);
                using (HttpWebResponse webRes = (HttpWebResponse)webReq.GetResponse()) // get web response
                {
                    // open a streamreader to read response content
                    using (System.IO.StreamReader mystream = new StreamReader(webRes.GetResponseStream()))
                    {
                        // htmldocument is used for seeing html code as xml code and selecting spesific parts easily
                        // it decreases time to extract the parts we want from html
                        HtmlDocument doc = new HtmlDocument();
                        // if there is a stream load the html content
                        if (mystream != null)
                        {
                            doc.Load(mystream);
                        }

                        // create a collection of nodes which holds chapters division in html code
                        HtmlNodeCollection coll = doc.DocumentNode.SelectNodes("//div[@id='chapters']//ul[@class='chlist']//li//div");
                        if (coll != null) // if there are chapters which means collection is not empty, continue
                        {
                            // foreach node in collection, get info about chapter such as
                            for (int i = coll.Count - 1; i >= 0; i--)
                            {
                                ChapterModel chptr = new ChapterModel();
                                String name1;
                                // get the extended name of the chapter such as Chapter 13 - !""This part""!
                                HtmlNode node1 = coll[i].SelectSingleNode("h3//span[@class='title nowrap'] | h4//span[@class='title nowrap']");
                                if (node1 == null) // if extended name doesnt exists set name1 to null
                                    name1 = null;
                                else // otherwise get the name and store it in name1 field
                                    name1 = coll[i].SelectSingleNode("h3//span[@class='title nowrap'] | h4//span[@class='title nowrap']").InnerText;
                                // get the number of  the chapter such as !""This Part""! - Hello World and store it in name2 field
                                String name2 = coll[i].SelectSingleNode("h3//a | h4//a").InnerText.RemoveExtaSpaces();
                                // merge the name, if name1 exsists, store them in one string-name-, if it does not store only name2
                                chptr.Name = (name1 == null ? name2.RemoveExtaSpaces() : (name2.RemoveExtaSpaces() + " : " + name1.RemoveExtaSpaces()));
                                // get link of the chapter from nodes
                                chptr.Link = coll[i].SelectSingleNode("h3//a | h4//a").Attributes["href"].Value;
                                // get unique id of the manga and set it to this chapter
                                chptr.Id = model.Id;
                                // get release date of the chapter
                                // TODO
                                // convert 'today', 'yesterday', and these things to the date later
                                chptr.Date = coll[i].SelectSingleNode("span[@class='date']").InnerText;
                                // set parent of the chapter to the manga
                                chptr.Parent = model;
                                // if there is a chapter such as this one dont add
                                if (model.Items.Contains(chptr) == true)
                                    continue;
                                else
                                    chptr.IsNew = true;
                                // add chapter to the model
                                model.Items.Add(chptr);
                            }
                            model.Count = model.Items.Count;
                        }
                        if(model.InfoState < 1)
                            model.InfoState = 1;
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }        

        #endregion
    }

    public class FileInfoComparer : IComparer<FileInfo>
    {
        public int Compare(FileInfo x, FileInfo y)
        {
            int a = int.Parse(Path.GetFileNameWithoutExtension(x.Name));
            int b = int.Parse(Path.GetFileNameWithoutExtension(y.Name));

            return a.CompareTo(b);
        }
    }
}
