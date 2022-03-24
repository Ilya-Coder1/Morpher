using System;
using System.Collections.Generic;

namespace Morphology
{
    public class SentenceMorpher
    {
        private Dictionary<string, List<int>> _words;
        private Dictionary<int, List<(string, HashSet<string>)>> _groups;

        /// <summary>
        ///     Создает <see cref="SentenceMorpher"/> из переданного набора строк словаря.
        /// </summary>
        /// <remarks>
        ///     В этом методе должен быть код инициализации: 
        ///     чтение и преобразование входных данных для дальнейшего их использования
        /// </remarks>
        /// <param name="dictionaryLines">
        ///     Строки исходного словаря OpenCorpora в формате plain-text.
        ///     <code> СЛОВО(знак_табуляции)ЧАСТЬ РЕЧИ( )атрибут1[, ]атрибут2[, ]атрибутN </code>
        /// </param>
        /// 

        public static SentenceMorpher Create(IEnumerable<string> dictionaryLines)
        {
            int group = 1;
            var words = new Dictionary<string, List<int>>();
            var groups = new Dictionary<int, List<(string, HashSet<string>)>>();
            var attributes = new List<(string, HashSet<string>)>();
            var allAttributes = new HashSet<string>();

            foreach (var line in dictionaryLines)
            {
                if (!String.IsNullOrEmpty(line))
                {
                    if (int.TryParse(line, out int id))
                    {
                        attributes = new List<(string, HashSet<string>)>();
                        group = id;
                    }
                    else
                    {
                        string word = ParseString(line, out string[] attribute);

                        words.TryAdd(word, new List<int>());
                        words[word].Add(group);

                        attributes.Add((word, ConvertArrayToHashSet(attribute, allAttributes)));
                    }
                }
                else
                {
                    groups.TryAdd(group, attributes);
                }
            }

            return new SentenceMorpher() { _groups = groups, _words = words };
        }

        private static string ParseString(string stringToParse, out string[] attributes)
        {
            var wordAndAttributes = stringToParse.Split('\t');
            string word = wordAndAttributes[0];
            attributes = wordAndAttributes[1].ToLower().Split(' ', ',');

            return word;
        }

        private static HashSet<string> ConvertArrayToHashSet(string[] attributes, HashSet<string> allAtributes)
        {
            HashSet<string> result = new HashSet<string>();

            foreach (var attribute in attributes)
            {
                if (!allAtributes.TryGetValue(attribute, out var atrbt))
                {
                    allAtributes.Add(attribute); 
                    result.Add(attribute);
                }
                else
                    result.Add(atrbt);
            }

            return result;
        }

        /// <summary>
        ///     Выполняет склонение предложения согласно указанному формату
        /// </summary>
        /// <param name="sentence">
        ///     Входное предложение <para/>
        ///     Формат: набор слов, разделенных пробелами.
        ///     После слова может следовать спецификатор требуемой части речи (формат описан далее),
        ///     если он отсутствует - слово требуется перенести в выходное предложение без изменений.
        ///     Спецификатор имеет следующий формат: <code>{ЧАСТЬ РЕЧИ,аттрибут1,аттрибут2,..,аттрибутN}</code>
        ///     Если для спецификации найдётся несколько совпадений - используется первое из них
        /// </param>

        public virtual string Morph(string sentence)
        {
            var wordsToMorph = sentence.Split(' ', '\n');

            string result = "";

            foreach (var item in wordsToMorph)
            {
                var wordAndAttributes = item.Split('{', '}');
                string word = wordAndAttributes[0].ToUpper();

                if (wordAndAttributes.Length == 1)
                {
                    result += word + " ";
                    continue;
                }

                if (String.IsNullOrEmpty(wordAndAttributes[1]))
                {
                    result += word + " ";
                    continue;
                }

                var attributes = new HashSet<string>(wordAndAttributes[1].ToLower().Split(',', ' '));

                var isFounded = false;

                if (_words.TryGetValue(word, out var groupsNumbers))
                {
                    foreach (var groupNumber in groupsNumbers)
                    {
                        foreach (var morphem in _groups[groupNumber])
                        {
                            if (attributes.IsSubsetOf(morphem.Item2))
                            {
                                result += morphem.Item1 + " ";
                                isFounded = true;
                                break;
                            }
                        }
                        if (isFounded)
                            break;
                    }
                }

                if (!isFounded)
                    result += word + " ";
            }

            return result.Trim();
        }
    }

    //public class TrieNode<TValue> where TValue: class
    //{
    //    private static char rootKey = '~';
    //    char key;
    //    TValue? value;
    //    List<TrieNode<TValue>>? children;

    //    static public TrieNode<TValue> CreateRootNode()
    //    {
    //        return new TrieNode<TValue>(rootKey, null);
    //    }

    //    public TrieNode(char key, TValue? value)
    //    {
    //        this.key = key;
    //        this.value = value;
    //    }

    //    public void Add(string key, TValue value)
    //    {
    //        TValue? vl = null;

    //        if (key.Length == 1)
    //            vl = value;

    //        if (String.IsNullOrEmpty(key))
    //            return;

    //        if (children == null)
    //            children = new List<TrieNode<TValue>>();

    //        char c = key[0];
    //        TrieNode<TValue>? child = FindChar(c);
    //        if (child == null)
    //        {
    //            child = new TrieNode<TValue>(c, vl);
    //            children.Add(child);
    //        }

    //        child.Add(key.Substring(1), value);
    //    }

    //    private TrieNode<TValue>? FindChar(char c)
    //    {
    //        if (children != null)
    //        {
    //            foreach (var node in children)
    //            {
    //                if (node.key == c)
    //                    return node;
    //            }
    //        }

    //        return null;
    //    }

    //    public TrieNode<TValue>? FindString(string str)
    //    {
    //        TrieNode<TValue>? current = this;

    //        for (int i = 0; i < str.Length; i++)
    //        {
    //            current = current.FindChar(str[i]);
    //            if (current == null)
    //                return null;
    //        }

    //        return current;
    //    }

    //    public TValue? GetValue(string key)
    //    {
    //        TrieNode<TValue>? node = FindString(key);

    //        if (node == null)
    //            return null;

    //        return node.value;
    //    }

    //    public void GetAllStrings(string prefix, List<string> strings)
    //    {
    //        if (strings is null)
    //            strings = new List<string>();

    //        if (key != rootKey)
    //        {
    //            prefix += key;
    //        }

    //        if (children != null)
    //        {
    //            foreach (var node in children)
    //            {
    //                node.GetAllStrings(prefix, strings);
    //            }
    //        }
    //        else
    //        {
    //            strings.Add(prefix);
    //        }
    //    }
    //}

}
