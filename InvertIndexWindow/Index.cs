using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace InvertIndexWindow
{
    class Index
    {
        // collection for invert index
        public Dictionary<string, List<string>> Collection { get; private set; }
        // list of books title
        public List<string> bookTitlesList;
        public List<string> allBooksId;

        public Index()
        {
            Collection = new Dictionary<string, List<string>>();
            bookTitlesList = new List<string>();
            allBooksId = new List<string>();
        }

        // sets the index from a number of files 
        private void CreateIndex(List<string> files)
        {
            int i = 0;
            foreach (var file in files)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(file);

                bookTitlesList.Add(doc.GetElementsByTagName("book-title")[0].InnerText);

                XmlNodeList elemList = doc.GetElementsByTagName("p");

                foreach (XmlNode elem in elemList)
                {
                    foreach (var word in GetWords(elem.InnerText))
                    {
                        List<string> temp = new List<string>();
                        temp.Add(i.ToString());
                        if (!Collection.ContainsKey(word.ToLower()))
                            Collection.Add(word.ToLower(), temp);
                        else if (!Collection[word.ToLower()].Contains(i.ToString()))
                            Collection[word.ToLower()].Add(i.ToString());
                    }
                }

                allBooksId.Add(i.ToString());
                i++;
            }
        }

        // saves index to .xml file
        public void CreateIndexFile(List<string> files , string toFile)
        {
            CreateIndex(files);

            XDocument doc = new XDocument(new XElement("index"));

            XElement metaEl = new XElement("meta");
            foreach (var title in bookTitlesList)
            {
                XElement bookTitle = new XElement("title", title);
                metaEl.Add(bookTitle);
            }

            doc.Root.Add(metaEl);

            var keys = Collection.Keys.ToList();
            keys.Sort();

            foreach (var key in keys)
            {
                XElement wEl = new XElement("w");
                XElement titleEl = new XElement("key", key);
                XElement listEl = new XElement("list");

                wEl.Add(titleEl);
                wEl.Add(listEl);

                foreach (var index in Collection[key])
                {
                    listEl.Add(new XElement("book", index));
                }

                doc.Root.Add(wEl);
            }

            doc.Save(toFile);
        }

        // loads index from .xml file to the structure
        private Dictionary<string, List<string>> LoadIndexFile(string path)
        {
            XDocument doc = XDocument.Load(path);
            Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();

            XElement metaElement = doc.Root.Element("meta");

            foreach (var bookTitle in metaElement.Elements("title"))
            {
                bookTitlesList.Add(bookTitle.Value);
            }

            foreach (var elem in doc.Root.Elements("w"))
            {
                XElement word = elem.Element("key");
                List<string> books = new List<string>();

                foreach (var book in elem.Element("list").Elements("book"))
                {
                    books.Add(book.Value);
                }

                dictionary.Add(word.Value, books);
            }

            return dictionary;
        }


        public void LoadIndex(string path)
        {
            Collection = LoadIndexFile(path);
        }

        // returns a list of books id for single word from index
        private List<string> SearchWord(string query)
        {
            List<string> value = new List<string>();

            if (Collection.TryGetValue(query, out value))
            {
                return Collection[query];
            }
            else
            {
                return new List<string>();
            }

        }

        // returns a list of books id for bool query
        public List<string> Search(string query)
        {
            List<string> result = new List<string>();

            List<string> tokens = query.Split().ToList();
            List<string> words = new List<string>();
            List<string> symbols = new List<string>();

            foreach (var token in tokens)
            {
                if (!(token.Equals("&") || token.Equals("|")))
                {
                    words.Add(token);
                }
                else
                {
                    symbols.Add(token);
                }
            }

            result = SearchWord(words[0]);

            for (int i = 1, j = 0; j < symbols.Count; i++, j++)
            {
                if (symbols[j].Equals("&"))
                {
                    result = result.Intersect(SearchWord(words[i])).ToList();
                }
                else if (symbols[j].Equals("|"))
                {
                    result = result.Concat(SearchWord(words[i])).Distinct().ToList();
                }
            }

            result.Sort();

            return GetBookTitles(result);
        }

        // returns titles of books by their id
        private List<string> GetBookTitles(List<string> list)
        {
            List<string> result = new List<string>();

            for (int i = 0; i < list.Count; i++)
            {
                int tempIndex = Int32.Parse(list[i]);
                result.Add(bookTitlesList[tempIndex]);
            }

            return result;
        }

        // string parsing
        private string[] GetWords(string input)
        {
            MatchCollection matches = Regex.Matches(input, @"\w+");

            var words = from m in matches.Cast<Match>()
                        where !string.IsNullOrEmpty(m.Value)
                        select m.Value;

            return words.ToArray();
        }

        public void CollectionsToDefault()
        {
            Collection = new Dictionary<string, List<string>>();
            bookTitlesList = new List<string>();
            allBooksId = new List<string>();
        }
    }
}
