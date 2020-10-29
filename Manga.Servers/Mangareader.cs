using HtmlAgilityPack;
using Manga.Framework;
using Manga.Interfaces;
using Manga.Structures.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Manga.Servers
{
    public class Mangareader : ObservableObject, IServer
    {
        #region Members

        private bool _isRunning;
        System.Text.RegularExpressions.Regex regex;
        string sizepattern = "(?<=of )([0-9]*)";

        #endregion

        #region Properties

        public bool IsRunning
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes and compiles a new instace of Manga.Servers.Mangareader object class.
        /// </summary>
        public Mangareader()
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

                    HtmlNode node = Doc.DocumentNode.SelectSingleNode("//div[@id='selectpage']");
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
            // cast mangamodel parameter
            MangaModel model = obj as MangaModel;

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
                            doc.Load(mystream);

                        // create a collection of nodes which holds chapters division in html code
                        HtmlNodeCollection coll =
                            doc.DocumentNode.SelectNodes("//div[@id='wrapper_body']//div[@id='chapterlist']//table//tr");
                        if (coll != null) // if there are chapters which means collection is not empty, continue
                        {
                            // foreach node in collection, get info about chapter such as
                            for (int i = 1; i < coll.Count; i++)
                            {
                                ChapterModel chptr = new ChapterModel();
                                // get the name of the chapter
                                chptr.Name = coll[i].SelectSingleNode("td[1]").InnerText.RemoveExtaSpaces();
                                // get link of the chapter from nodes
                                chptr.Link = string.Format("http://www.mangareader.net{0}",
                                    coll[i].SelectSingleNode("td[1]//a").Attributes["href"].Value);
                                // get unique id of the manga and set it to this chapter
                                chptr.Id = model.Id;
                                // get release date of the chapter
                                // TODO
                                // convert 'today', 'yesterday', and these things to the date later
                                chptr.Date = coll[i].SelectSingleNode("td[2]").InnerText;
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

                        // get the image link
                        model.ImageSource = new Uri
                            (doc.DocumentNode.SelectSingleNode("//div[@id='wrapper_body']//div[@id='bodyust']//div[@id='mangaimg']//img").Attributes["src"].Value);

                        // get the manga properties
                        coll = doc.DocumentNode.SelectNodes("//div[@id='wrapper_body']//div[@id='bodyust']//div[@id='mangaproperties']//table//tr");

                        //get the alternative name
                        model.AlternativeName = coll[1].SelectSingleNode("td[2]").InnerText;

                        //get the release year
                        model.Year = coll[2].SelectSingleNode("td[2]").InnerText;

                        //get the author name
                        model.Author = coll[4].SelectSingleNode("td[2]").InnerText;

                        // get the artist name
                        model.Artist = coll[5].SelectSingleNode("td[2]").InnerText;

                        HtmlNodeCollection genrecol = coll[7].SelectNodes("td[2]//a");

                        // get genres
                        if (genrecol != null)
                        {
                            model.Genres = new string[genrecol.Count];
                            for (int i = 0; i < genrecol.Count; i++)
                            {
                                model.Genres[i] = genrecol[i].InnerText;
                            }
                        }

                        // get the description
                        model.Description = 
                            doc.DocumentNode.SelectSingleNode("//div[@id='wrapper_body']//div[@id='readmangasum']//p").InnerText;
                    }
                }
                model.InfoState = 2;
            }
            catch (Exception) { }
        }

        #endregion

        #region Info - Download

        public void GetInfoMinimal(IMangaModel model, bool State)
        {
            // create another instance of server which allows us to get info from seperate thread
            model.Publisher = Publisher.Mangareader;

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
                        HtmlNodeCollection coll = 
                            doc.DocumentNode.SelectNodes("//div[@id='wrapper_body']//div[@id='chapterlist']//table//tr");
                        if (coll != null) // if there are chapters which means collection is not empty, continue
                        {
                            // foreach node in collection, get info about chapter such as
                            for (int i = 1; i < coll.Count; i++)
                            {
                                ChapterModel chptr = new ChapterModel();
                                // get the name of the chapter
                                chptr.Name = coll[i].SelectSingleNode("td[1]").InnerText.RemoveExtaSpaces();
                                // get link of the chapter from nodes
                                chptr.Link = string.Format("http://www.mangareader.net{0}",
                                    coll[i].SelectSingleNode("td[1]//a").Attributes["href"].Value);
                                // get unique id of the manga and set it to this chapter
                                chptr.Id = model.Id;
                                // get release date of the chapter
                                // TODO
                                // convert 'today', 'yesterday', and these things to the date later
                                chptr.Date = coll[i].SelectSingleNode("td[2]").InnerText;
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
            string dir = string.Format("{0}\\{1}\\{2}", "D:\\Manga",
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

            string link = item.Link;

            for (int i = 1; i <= item.Size; i++)
            {
                if (state)
                    break;

                HtmlDocument doc = new HtmlDocument();
                int j = item.Link.LastIndexOf('/');
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


                        HtmlNode node = doc.DocumentNode.SelectSingleNode("//div[@id='container']//div[@id='imgholder']//a");
                        link = string.Format("http://www.mangareader.net{0}", node.Attributes["href"].Value);
                        imagelink = node.SelectSingleNode("img").Attributes["src"].Value;
                        mystream.Close();
                    }
                    webRes.Close();
                }

                DownloadFile file = new DownloadFile();
                file.PathToSave = string.Format("{0}\\{1}.jpg", dir, i);
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

        public void FetchList(object obj, System.Threading.CancellationToken Token)
        {
            // cast first parameter to threadsafeobservablecollection
            ThreadSafeObservableCollection<MangaModel> model = obj as ThreadSafeObservableCollection<MangaModel>;

            try
            {
                Token.ThrowIfCancellationRequested();

                // create a webrequest to manga directory of publisher
                HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create("http://www.mangareader.net/alphabetical");
                webReq.KeepAlive = true; // keep alive to prevent timeout

                Token.ThrowIfCancellationRequested();

                using (HttpWebResponse webRes = (HttpWebResponse)webReq.GetResponse()) // get web response
                {   // open a streamreader to read response content

                    Token.ThrowIfCancellationRequested();

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
                        Token.ThrowIfCancellationRequested();
                        // select the manga list div field and get all the links from it
                        HtmlNodeCollection col =
                            doc.DocumentNode.SelectNodes("//div[@id='wrapper_body']//div[@class='content_bloc2']//div[@class='series_col']/div[@class='series_alpha']/ul/li/a");
                        // create an array of mangamodel[number of nodes in col]
                        List<MangaModel> items = new List<MangaModel>();
                        // for each nodes in collection create a manga and fill the manga information and add it to the array
                        for (int i = 0; i < col.Count; i++)
                        {
                            Token.ThrowIfCancellationRequested();

                            MangaModel manga = new MangaModel();

                            manga.Name = col[i].InnerText;
                            manga.Link = string.Format("http://www.mangareader.net{0}", col[i].Attributes["href"].Value);
                            manga.Publisher = Publisher.Mangareader;
                            if (model.Contains(manga))
                                continue;
                            items.Add(manga);
                        }
                        Token.ThrowIfCancellationRequested();
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
            catch (Exception)
            {

            }
        }

        #region Reader

        public void FindImages(IReaderModel model, bool State)
        {
            for (int i = 0; i < model.Items.Count; i++)
            {
                FindImages(model.Items[i], State);
            }
        }

        public void FindImages(IChapterModel model, bool State)
        {           
            string dir = string.Format("{0}\\{1}\\{2}", Manga.Structures.Properties.Settings.Default.DownloadPath,
                    model.Parent.Name.Trim().RemoveIllegalCharacters(),
                    model.Name.Trim().RemoveIllegalCharacters());

            System.IO.Directory.CreateDirectory(dir);

            model.Size = GetChapterSize(model.Link);

            PriorityQueue<DownloadFile, Priority> _queue = null;

            if (model.Size > 0)
            {
                _queue = new PriorityQueue<DownloadFile, Priority>() { Priority = Priority.Read };
                ParallelWorker.InstanceDownload.EnqueueFileToDownload(_queue);
            }

            Database.DatabaseEngine.Instance.Update(model);

            string link = model.Link;

            for (int i = 1; i <= model.Size; i++)
            {
                model.Items.Add(new ImageModel());

                HtmlDocument doc = new HtmlDocument();
                int j = model.Link.LastIndexOf('/');
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

                        HtmlNode node = doc.DocumentNode.SelectSingleNode("//div[@id='container']//div[@id='imgholder']//a");
                        link = string.Format("http://www.mangareader.net{0}", node.Attributes["href"].Value);
                        imagelink = node.SelectSingleNode("img").Attributes["src"].Value;
                    }
                    webRes.Close();
                }

                DownloadFile file = new DownloadFile();
                file.PathToSave = string.Format("{0}\\{1}.jpg", dir, i);
                file.Url = imagelink;
                file.Priority = Priority.Read;
                file.Index = i - 1;
                file.FinishedEvent += new DownloadFinishedEventHandler((sender) =>
                {
                    lock (model.Items)
                    {
                        model.Items[sender.Index].Link = sender.PathToSave;
                        model.Items[sender.Index].LinkType = LinkType.Local;
                    }
                    if (sender.Index == 0)
                        (model.Parent as ReaderModel).ItemsCreated.Invoke();
                });

                _queue.Enqueue(file, Priority.Read);
            }

        }

        #endregion

        public void CheckUpdates(IMangaModel obj, System.Threading.CancellationToken Token)
        {
            
        }
    }
}
