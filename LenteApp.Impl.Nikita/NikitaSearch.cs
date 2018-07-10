using LenteApp.LibraryBase;
using System;
using System.Collections.Generic;
using System.IO;

namespace LenteApp.Impl.Nikita
{
    public class NikitaSearch : ISearchImplBase
    {
        public static string DataDirectory => Path.Combine(Environment.CurrentDirectory, "Data", "Nikita");

        public static int LevenshteinDistance(string string1, string string2)
        {
            if (string1 == null)
            {
                throw new ArgumentNullException(nameof(string1));
            }

            if (string2 == null)
            {
                throw new ArgumentNullException(nameof(string2));
            }

            int[,] m = new int[string1.Length + 1, string2.Length + 1];

            for (int i = 0; i <= string1.Length; i++)
            {
                m[i, 0] = i;
            }

            for (int j = 0; j <= string2.Length; j++)
            {
                m[0, j] = j;
            }

            for (int i = 1; i <= string1.Length; i++)
            {
                for (int j = 1; j <= string2.Length; j++)
                {
                    var diff = (string1[i - 1] == string2[j - 1]) ? 0 : 1;

                    m[i, j] = Math.Min(Math.Min(m[i - 1, j] + 1,
                            m[i, j - 1] + 1),
                        m[i - 1, j - 1] + diff);
                }
            }

            return m[string1.Length, string2.Length];
        }

        private readonly Dictionary<char, HashSet<string>> wordDictionary = new Dictionary<char, HashSet<string>>();
        private readonly Dictionary<string, string> normalDictionary = new Dictionary<string, string>();
        private readonly Dictionary<char, char> substitution = new Dictionary<char, char>();
        private readonly List<MatchResult> highestMatch = new List<MatchResult>();
        private readonly Dictionary<string, string> fileWordsDictionary = new Dictionary<string, string>();

        private readonly Dictionary<string, HashSet<string>> fileNormalWordsDictionary =
            new Dictionary<string, HashSet<string>>();
        private readonly List<string> newFilesPaths = new List<string>();

        public void AddFileToIndex(string filePath)
        {

            newFilesPaths.Add(filePath);
        }

        public List<SearchResult> DoSearch(string searchLine)
        {
            List<MatchResult> highestConcentration = new List<MatchResult>();
            List<MatchResult> matches = new List<MatchResult>();
            string[] searchWords = searchLine.Split(' ');
            DirectoryInfo dataDirectoryInfo = new DirectoryInfo(Path.Combine(DataDirectory, "faili"));
            foreach (var file in dataDirectoryInfo.GetFiles())
            {
                newFilesPaths.Add(file.FullName);
            }

            foreach (var fileName in newFilesPaths)
            {
                highestConcentration.Add(ProcessArticle(fileName, searchWords));
            }

            highestConcentration.Sort();
            highestConcentration.Reverse();
            matches.Sort();
            matches.Reverse();

            List<string> results = new List<string>();
            foreach (MatchResult match in matches)
            {
                if (match.MatchScore != 0)
                {
                    results.Add(match.FileName);
                }
            }

            highestConcentration.Sort();
            highestConcentration.Reverse();
            List<SearchResult> higheSearchResults = new List<SearchResult>();
            foreach (var value in highestConcentration)
            {
                SearchResult resultValue = new SearchResult();
                resultValue.Score = (uint)value.MatchScore;
                resultValue.BestContentExtract = value.MatchContent;
                resultValue.FilePath = value.FileName;
                higheSearchResults.Add(resultValue);
            }
            return higheSearchResults;
        }

        private void LoadWordDictionary()
        {
            for (char letter = 'а'; letter <= 'я'; letter++)
            {
                wordDictionary.Add(letter, new HashSet<string>());
            }

            wordDictionary.Add('ё', new HashSet<string>());

            using (FileStream fs = new FileStream(Path.Combine(DataDirectory, "Slovar1.txt"), FileMode.Open, FileAccess.Read))
            using (TextReader wordDictionaryReader = new StreamReader(fs))
            {
                string dictionaryLine = wordDictionaryReader.ReadLine();
                while (dictionaryLine != null)
                {
                    string[] dictionaryWords =
                        dictionaryLine.Split(new char[] { ' ', '*' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string dictionaryWord in dictionaryWords)
                    {
                        if (!normalDictionary.ContainsKey(dictionaryWord))
                        {
                            if (wordDictionary.ContainsKey(dictionaryWord[0]))
                            {
                                wordDictionary[dictionaryWord[0]].Add(dictionaryWord);
                            }

                            normalDictionary.Add(dictionaryWord, dictionaryWords[0]);
                        }
                    }

                    dictionaryLine = wordDictionaryReader.ReadLine();
                }
            }
        }

        public void Initialize()
        {
            LoadTranslitTable();

            LoadWordDictionary();
        }

        private MatchResult ProcessArticle(string file, string[] searchWords)
        {
            string fileWords;
            HashSet<string> fileNormalWords;
            if (!fileNormalWordsDictionary.ContainsKey(file))
            {
                var fileNormalWordList =
                    LoadArticleWords(file, out var fileWordsBlock, out fileWords);
                fileNormalWords = new HashSet<string>(fileNormalWordList);
                fileWordsDictionary.Add(file, fileWords);
                fileNormalWordsDictionary.Add(file, fileNormalWords);
            }
            else
            {
                fileWords = fileWordsDictionary[file];
                fileNormalWords = fileNormalWordsDictionary[file];
            }
            return Search(searchWords, fileNormalWords, file, fileWords);
        }

        private MatchResult Search(string[] searchWords, HashSet<string> fileNormalWords, string file, string fileWords)
        {
            int matchCount = 0;
            HashSet<string> normalLines = new HashSet<string>();
            List<string> highestMatchesList = new List<string>();
            HashSet<string> highestNormalMatchesHash = new HashSet<string>();
            List<string> translitSearchWordsInLowerCaseList = new List<string>();
            highestMatch.Clear();
            foreach (string searchWord in searchWords)
            {
                bool translitFailure = true;
                var cycleSkip = true;
                List<char> letters = new List<char>();
                foreach (char letterInSearchWord in searchWord)
                {
                    translitFailure = true;
                    if (substitution.ContainsKey(letterInSearchWord))
                    {
                        letters.Add(substitution[letterInSearchWord]);
                        translitFailure = false;
                    }
                }

                var translitSearchWord = searchWord;
                if (!translitFailure)
                {
                    translitSearchWord = new string(letters.ToArray());
                }

                var translitSearchWordLowerCase = translitSearchWord.ToLower();
                if (normalDictionary.ContainsKey(translitSearchWordLowerCase))
                {
                    if (fileNormalWords.Contains(normalDictionary[translitSearchWordLowerCase]))
                    {
                        matchCount++;
                        bool cycleBreak = true;
                        foreach (string normalKey in normalDictionary.Keys)
                        {
                            if (normalKey.StartsWith(translitSearchWord))
                            {
                                normalLines.Add(normalDictionary[normalKey]);
                                cycleBreak = false;
                                continue;
                            }

                            if (!cycleBreak)
                            {
                                break;
                            }
                        }
                    }
                }
                else
                {
                    char[] translitSearchWordLowerLetters = translitSearchWordLowerCase.ToCharArray();
                    double i = translitSearchWordLowerLetters.Length;

                    HashSet<string> sameTranslitSearchWordLowerLetterWords =
                        wordDictionary[translitSearchWordLowerLetters[0]];
                    foreach (var sameTranslitSearchWordLowerLetterWord in sameTranslitSearchWordLowerLetterWords)
                    {
                        if (!cycleSkip)
                        {
                            continue;
                        }

                        double livenshteinDistance = (LevenshteinDistance(translitSearchWordLowerCase,
                            sameTranslitSearchWordLowerLetterWord));
                        var percentageOfNonCoincidenceOfWord = livenshteinDistance / i;
                        if (percentageOfNonCoincidenceOfWord < 0.25)
                        {
                            translitSearchWordLowerCase = sameTranslitSearchWordLowerLetterWord;
                            cycleSkip = false;
                        }
                    }
                }

                if (fileNormalWords.Contains(normalDictionary[translitSearchWordLowerCase]))
                {
                    translitSearchWordsInLowerCaseList.Add(normalDictionary[translitSearchWordLowerCase]);
                    matchCount++;
                    bool cycleBreak = true;
                    foreach (string normalWord in normalDictionary.Keys)
                    {
                        if (normalWord.StartsWith(translitSearchWord))
                        {
                            normalLines.Add(normalDictionary[normalWord]);
                            cycleBreak = false;
                            continue;
                        }

                        if (!cycleBreak)
                        {
                            break;
                        }
                    }
                }
            }


            foreach (string normalWords in normalLines)
            {
                if (fileNormalWords.Contains(normalWords))
                {
                    matchCount++;
                }
            }

            string[] fileLines = fileWords.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var fileLine in fileLines)
            {
                var j = 0;
                string[] fileWordsInLine =
                    fileLine.Split(
                        new char[]
                        {
                            ' ', ',', '.', '!', '?', ':', ';', '(', ')', '№', '/', '1', '2', '3', '4', '5', '6', '7',
                            '8', '9', '0', '<', '>', '"'
                        }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var fileWord in fileWordsInLine)
                {
                    highestMatchesList.Add(fileWord);
                    if (normalDictionary.ContainsKey(fileWord))
                    {
                        highestNormalMatchesHash.Add(normalDictionary[fileWord]);
                    }
                }

                foreach (var normalMatchesWords in highestNormalMatchesHash)
                {
                    foreach (var searchNormalWord in translitSearchWordsInLowerCaseList)
                    {
                        if (normalMatchesWords == searchNormalWord)
                        {
                            j++;
                        }
                    }
                }

                MatchResult highestMatchResult = new MatchResult();
                highestMatchResult.MatchContent = fileLine;
                highestMatchResult.MatchHighestScore = j;
                highestMatchResult.MatchScore = matchCount;
                highestMatchResult.FileName = file;
                highestMatch.Add(highestMatchResult);
                highestMatchesList.Clear();
                highestNormalMatchesHash.Clear();
            }

            highestMatch.Sort();
            highestMatch.Reverse();
            return highestMatch[0];
        }

        public List<string> LoadArticleWords(string file, out string[] fileWordsBlock, out string fileWords)
        {
            FileStream fs1 = new FileStream(file, FileMode.Open, FileAccess.Read);
            TextReader reader = new StreamReader(fs1);
            fileWords = reader.ReadToEnd();
            fileWordsBlock =
                fileWords.Split(
                    new char[]
                    {
                        ' ', ',', '.', '!', '?', ':', ';', '(', ')', '№', '/', '1', '2', '3', '4', '5', '6', '7',
                        '8', '9', '0', '<', '>', '"'
                    }, StringSplitOptions.RemoveEmptyEntries);
            return NormalizeWords(fileWordsBlock);
        }

        private List<string> NormalizeWords(string[] fileWordsBlock)
        {
            List<string> fileNormalWords = new List<string>();
            foreach (string fileWord in fileWordsBlock)
            {
                string fileWordLowerCase = fileWord.ToLower();
                if (normalDictionary.ContainsKey(fileWordLowerCase))
                {
                    fileWordLowerCase = normalDictionary[fileWordLowerCase];
                }

                if (normalDictionary.ContainsKey(fileWordLowerCase))
                {
                    fileNormalWords.Add(normalDictionary[fileWordLowerCase]);
                }
            }

            return fileNormalWords;
        }

        private void LoadTranslitTable()
        {
            substitution.Add(' ', ' ');
            using (FileStream fs2 = new FileStream(Path.Combine(DataDirectory, "bikvi.txt"), FileMode.Open, FileAccess.Read))
            using (TextReader translitReader = new StreamReader(fs2))
            {
                string translitLetters = translitReader.ReadLine();
                while (translitLetters != null)
                {
                    substitution.Add(translitLetters[0], translitLetters[2]);
                    translitLetters = translitReader.ReadLine();
                }
            }
        }
    }
}