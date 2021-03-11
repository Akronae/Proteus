using System.Collections;
using System.Collections.Generic;
using Chresimos.Core.Utils;
using Xunit;

namespace Proteus.Test.Serializer
{
    public class Collection
    {
        [Fact]
        public void List ()
        {
            AssertList(new List<int>());
            AssertList(new List<int> {1, 2, 3, 4, -1, 0, int.MaxValue, int.MinValue});
            AssertList(new List<string> {null, "lala", null, "lolo", ""});
            AssertList(new List<List<bool>>
                {new List<bool>(), new List<bool> {true, false, true}, new List<bool> {true}});
            AssertList(new List<Dictionary<int, string>>
            {
                new Dictionary<int, string> {{0, "lala"}, {-54, "lolo"}},
                new Dictionary<int, string> {{int.MinValue, "lala"}, {int.MaxValue, null}}
            });
        }

        private static void AssertList <T> (List<T> value)
        {
            var serializer = new Core.Serializer();
            var serialized = serializer.Serialize(value);
            var deserialized = serializer.Deserialize<List<T>>(serialized);

            Assert.True(ListComparer(value, deserialized));
        }

        private static bool ListComparer (IList listA, IList listB)
        {
            if (listA.Count != listB.Count) return false;

            var listAGenericType = listA.GetListElementTypeOrDefault();
            var listBGenericType = listB.GetListElementTypeOrDefault();

            if (listAGenericType != listBGenericType) return false;

            foreach (var elementA in listA)
            {
                var elementB = listB[listA.IndexOf(elementA)];

                if (elementA?.GetType().IsIListType() == true)
                {
                    var elementAList = (IList) elementA;
                    var elementBList = (IList) elementB;

                    if (!ListComparer(elementAList, elementBList))
                    {
                        return false;
                    }

                    continue;
                }

                if (elementA?.GetType().IsIDictionaryType() == true)
                {
                    var elementADic = (IDictionary) elementA;
                    var elementBDic = (IDictionary) elementB;


                    if (!DictionaryComparer(elementADic, elementBDic))
                    {
                        return false;
                    }

                    continue;
                }

                if (!Equals(elementA, elementB)) return false;
            }

            return true;
        }

        [Fact]
        public void Dictionary ()
        {
            AssertDictionary(new Dictionary<int, int>());
            AssertDictionary(new Dictionary<int, int> {{1, 2}, {3, 4}, {5, 6}, {int.MinValue, int.MaxValue}});
            AssertDictionary(new Dictionary<string, int> {{"1", 2}, {"5", 6}, {"int.MinValue", int.MaxValue}});
            AssertDictionary(new Dictionary<bool, float> {{false, float.MaxValue}, {true, float.MinValue}});
            AssertDictionary(new Dictionary<List<int>, float>
                {{new List<int> {1, 2}, 2.5f}, {new List<int> {-2, 120}, -5.02f}});
            AssertDictionary(new Dictionary<Dictionary<string, int>, Dictionary<float, bool>>
            {
                {new Dictionary<string, int> {{"This", -1}}, new Dictionary<float, bool> {{-0.01f, false}}},
                {
                    new Dictionary<string, int> {{"Assert", int.MaxValue}},
                    new Dictionary<float, bool> {{-545.01f, true}}
                },
                {
                    new Dictionary<string, int> {{"Is", 654}, {"F*cked", -1}},
                    new Dictionary<float, bool> {{-0.01f, false}, {3f, true}}
                },
                {new Dictionary<string, int> {{"Up", 333}}, new Dictionary<float, bool> {{float.MinValue, true}}}
            });
        }

        private static void AssertDictionary <T1, T2> (Dictionary<T1, T2> value)
        {
            var serializer = new Core.Serializer();
            var serialized = serializer.Serialize(value);
            var deserialized = serializer.Deserialize<Dictionary<T1, T2>>(serialized);

            Assert.True(DictionaryComparer(value, deserialized));
        }

        private static bool DictionaryComparer (IDictionary dicA, IDictionary dicB)
        {
            if (dicA.Count != dicB.Count) return false;

            var dicAEnum = dicA.GetEnumerator();
            var dicBEnum = dicB.GetEnumerator();

            while (dicAEnum.MoveNext() && dicBEnum.MoveNext())
            {
                var keysAreEqual = false;
                var valuesAreEqual = false;

                if (dicAEnum.Key?.GetType().IsIListType() == true)
                {
                    if (!ListComparer((IList) dicAEnum.Key, (IList) dicBEnum.Key)) return false;
                    keysAreEqual = true;
                }

                if (dicAEnum.Value?.GetType().IsIListType() == true)
                {
                    if (!ListComparer((IList) dicAEnum.Value, (IList) dicBEnum.Value)) return false;
                    valuesAreEqual = true;
                }

                if (dicAEnum.Key?.GetType().IsIDictionaryType() == true)
                {
                    if (!DictionaryComparer((IDictionary) dicAEnum.Key, (IDictionary) dicBEnum.Key)) return false;
                    keysAreEqual = true;
                }

                if (dicAEnum.Value?.GetType().IsIDictionaryType() == true)
                {
                    if (!DictionaryComparer((IDictionary) dicAEnum.Value, (IDictionary) dicBEnum.Value)) return false;
                    valuesAreEqual = true;
                }

                if (!keysAreEqual && !Equals(dicAEnum.Key, dicBEnum.Key)) return false;
                if (!valuesAreEqual && !Equals(dicAEnum.Value, dicBEnum.Value)) return false;
            }

            return true;
        }
    }
}