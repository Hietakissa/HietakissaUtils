namespace HietakissaUtils
{
    using Random = UnityEngine.Random;
    using System.Collections.Generic;
    using System.IO.Compression;
    using System.Reflection;
    using Newtonsoft.Json;
    using Serialization;
    using System.Linq;
    using System.Text;
    using UnityEngine;
    using UnityEditor;
    using System;

    public static class Extensions
    {
        #region Vector2 Extensions
        #region Set
        public static Vector2 SetX(this Vector2 vector, float x)
        {
            vector.x = x;
            return vector;
        }
        public static Vector2 SetY(this Vector2 vector, float y)
        {
            vector.y = y;
            return vector;
        }
        public static Vector2 SetXY(this Vector2 vector, float x, float y)
        {
            vector.Set(x, y);
            return vector;
        }
        #endregion
        #region Operators
        public static Vector2 MultX(this Vector2 vector, float mult) => vector.SetX(vector.x * mult);
        public static Vector2 MultY(this Vector2 vector, float mult) => vector.SetY(vector.y * mult);

        public static Vector2 DivX(this Vector2 vector, float div) => vector.SetX(vector.x / div);
        public static Vector2 DivY(this Vector2 vector, float div) => vector.SetY(vector.y / div);

        public static Vector2 AddX(this Vector2 vector, float add) => vector.SetX(vector.x + add);
        public static Vector2 AddY(this Vector2 vector, float add) => vector.SetY(vector.y + add);

        public static Vector2 SubX(this Vector2 vector, float sub) => vector.SetX(vector.x - sub);
        public static Vector2 SubY(this Vector2 vector, float sub) => vector.SetY(vector.y - sub);
        #endregion
        #endregion
        #region Vector3 Extensions
        #region Set
        public static Vector3 SetX(this Vector3 vector, float value)
        {
            vector.x = value;
            return vector;
        }
        public static Vector3 SetY(this Vector3 vector, float value)
        {
            vector.y = value;
            return vector;
        }
        public static Vector3 SetZ(this Vector3 vector, float value)
        {
            vector.z = value;
            return vector;
        }
        public static Vector3 SetXY(this Vector3 vector, float x, float y)
        {
            vector.Set(x, y, vector.z);
            return vector;
        }
        public static Vector3 SetXZ(this Vector3 vector, float x, float z)
        {
            vector.Set(x, vector.y, z);
            return vector;
        }
        public static Vector3 SetYZ(this Vector3 vector, float y, float z)
        {
            vector.Set(vector.x, y, z);
            return vector;
        }
        public static Vector3 SetXYZ(this Vector3 vector, float x, float y, float z)
        {
            vector.Set(x, y, z);
            return vector;
        }
        #endregion
        #region Operators
        public static Vector3 MultX(this Vector3 vector, float mult) => vector.SetX(vector.x * mult);
        public static Vector3 MultY(this Vector3 vector, float mult) => vector.SetY(vector.y * mult);
        public static Vector3 MultZ(this Vector3 vector, float mult) => vector.SetZ(vector.z * mult);

        public static Vector3 DivX(this Vector3 vector, float div) => vector.SetX(vector.x / div);
        public static Vector3 DivY(this Vector3 vector, float div) => vector.SetY(vector.y / div);
        public static Vector3 DivZ(this Vector3 vector, float div) => vector.SetZ(vector.z / div);

        public static Vector3 AddX(this Vector3 vector, float add) => vector.SetX(vector.x + add);
        public static Vector3 AddY(this Vector3 vector, float add) => vector.SetY(vector.y + add);
        public static Vector3 AddZ(this Vector3 vector, float add) => vector.SetZ(vector.z + add);

        public static Vector3 SubtX(this Vector3 vector, float sub) => vector.SetX(vector.x - sub);
        public static Vector3 SubtY(this Vector3 vector, float sub) => vector.SetY(vector.y - sub);
        public static Vector3 SubtZ(this Vector3 vector, float sub) => vector.SetZ(vector.z - sub);
        #endregion
        #endregion
        #region Float Extensions
        public static int RoundUp(this float roundNum) => (int)Math.Ceiling(roundNum);
        public static int RoundDown(this float roundNum) => (int)Math.Floor(roundNum);
        public static int RoundToNearest(this float num) => Maf.RoundToNearest(num);
        public static float RoundBy(this float num, float roundBy) => Maf.RoundBy(num, roundBy);
        public static float RoundToDecimalPlaces(this float num, int decimalPlaces) => Maf.RoundToDecimalPlaces(num, decimalPlaces);

        public static float Abs(this float absFloat) => Mathf.Abs(absFloat);

        public static float FlipOne(this float num) => Maf.FlipOne(num);
        #endregion
        #region String Extensions
        public static string AddInFrontOfMatches(this string text, string textToAdd, params string[] matches)
        {
            foreach (string match in matches) text = text.Replace(match, $"{textToAdd}{match}");
            return text;
        }
        public static string ReplaceMultiple(this string text, string replacement, params string[] targets)
        {
            foreach (string target in targets) text.Replace(target, replacement);
            return text;
        }
        public static string ReplaceFirst(this string text, string match, string replacement)
        {
            int pos = text.IndexOf(match);

            if (pos < 0) return text;

            return text.Substring(0, pos) + replacement + text.Substring(pos + match.Length);
        }
        public static string Remove(this string targetString, string stringToRemove)
        {
            return targetString.Replace(stringToRemove, "");
        }
        public static string RemoveFirst(this string targetString, string stringToRemove)
        {
            return targetString.ReplaceFirst(stringToRemove, "");
        }
        #endregion

        public static int Abs(this int absInt)
        {
            return Mathf.Abs(absInt);
        }

        public static Quaternion ToQuaternion(this Vector3 euler)
        {
            return Quaternion.Euler(euler);
        }
        public static Vector3 ToEuler(this Quaternion quaternion)
        {
            return quaternion.eulerAngles;
        }

        public static int Normalized(this float num) => num > 0 ? 1 : (num < 0 ? -1 : 0);

        public static TElement RandomElement<TElement>(this TElement[] array) => array[Random.Range(0, array.Length)];
        public static TElement RandomElement<TElement>(this List<TElement> list) => list[Random.Range(0, list.Count)];

        public static void Shuffle<TElement>(this List<TElement> list)
        {
            for (int i = list.Count; i-- > 0;)
            {
                int k = Random.Range(0, i + 1);
                TElement temp = list[k];
                list[k] = list[i];
                list[i] = temp;
            }
        }
        public static void Shuffle<TElement>(this TElement[] array)
        {
            for (int i = array.Length; i-- > 0;)
            {
                int k = Random.Range(0, i + 1);
                TElement temp = array[k];
                array[k] = array[i];
                array[i] = temp;
            }
        }

        public static bool IndexInBounds<TType>(this TType[] array, int index) => index >= 0 && index < array.Length;
        public static bool IndexInBounds<TType>(this TType[,] array, int x, int y) => x >= 0 && x < array.GetLength(0) && y >= 0 && y < array.GetLength(1);
        public static bool IndexInBounds<TType>(this TType[,,] array, int x, int y, int z) => x >= 0 && x < array.GetLength(0) && y >= 0 && y < array.GetLength(1) && z >= 0 && z < array.GetLength(2);
        public static bool IndexInBounds<TType>(this List<TType> array, int index) => index >= 0 && index < array.Count;

        public static bool Contains<TType>(this TType[] array, TType value)
        {
            int length = array.Length;
            for (int i = 0; i < length; i++)
            {
                if (Equals(array[i], value)) return true;
            }
            return false;
        }
        public static bool Contains(this LayerMask mask, int layer)
        {
            return mask == (mask | (1 << layer));
        }

        public static void DestroyChildren(this Transform transform)
        {
            int childCount = transform.childCount;

            for (int i = childCount - 1; i >= 0; i--)
            {
                GameObject.Destroy(transform.GetChild(i).gameObject);
            }
        }
        public static void DestroyChildrenImmediate(this Transform transform)
        {
            int childCount = transform.childCount;

            for (int i = childCount - 1; i >= 0; i--)
            {
                GameObject.DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }
        public static void DestroyChildrenAuto(this Transform transform)
        {
            int childCount = transform.childCount;

            for (int i = childCount - 1; i >= 0; i--)
            {
                QOL.QOL.Destroy(transform.GetChild(i).gameObject);
            }
        }

        public static float CalculateVolume(this Mesh mesh)
        {
            const float multiplier = 0.1666666667f;
            float volume = 0;

            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;

            Vector3 p1;
            Vector3 p2;
            Vector3 p3;

            for (int i = 0; i < triangles.Length; i += 3)
            {
                p1 = vertices[triangles[i + 0]];
                p2 = vertices[triangles[i + 1]];
                p3 = vertices[triangles[i + 2]];
                volume += SignedVolumeOfTriangle(p1, p2, p3);
            }
            return Mathf.Abs(volume);

            float SignedVolumeOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
            {
                float v321 = p3.x * p2.y * p1.z;
                float v231 = p2.x * p3.y * p1.z;
                float v312 = p3.x * p1.y * p2.z;
                float v132 = p1.x * p3.y * p2.z;
                float v213 = p2.x * p1.y * p3.z;
                float v123 = p1.x * p2.y * p3.z;

                return multiplier * (-v321 + v231 + v312 - v132 - v213 + v123);
            }
        }

        public static void GetOrAddComponent<TType>(this Transform transform, out TType component) where TType : Component
        {
            if (!transform.TryGetComponent(out component)) component = transform.gameObject.AddComponent<TType>();
        }
        public static void GetOrAddComponent<TType>(this GameObject gameObject, out TType component) where TType : Component
        {
            if (!gameObject.TryGetComponent(out component)) component = gameObject.AddComponent<TType>();
        }

        public static Vector3 DirectionTo(this Transform t, Vector3 position) => Maf.Direction(t.position, position);

        public static Texture2D ToTexture2D(this RenderTexture rTex)
        {
            Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGB24, false);
            RenderTexture oldRT = RenderTexture.active;
            RenderTexture.active = rTex;

            tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
            tex.Apply();

            RenderTexture.active = oldRT;
            return tex;
        }

        public static Vector3 RandomPoint(this Bounds bounds) => Maf.RandomPointInBounds(bounds);

        public static Vector3 GetCanvasPositionForElement(this Canvas canvas, RectTransform uiElement) => canvas.transform.InverseTransformPoint(uiElement.position);

        public static T CopyTo<T>(this T original, GameObject destination) where T : Component
        {
            Type type = original.GetType();
            Component copy = destination.AddComponent(type);

            FieldInfo[] fields = type.GetFields();
            foreach (FieldInfo field in fields)
            {
                if (field.IsStatic) continue;
                field.SetValue(copy, field.GetValue(original));
            }

            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (!property.CanWrite || property.Name == "name" || property.IsDefined(typeof(ObsoleteAttribute), true)) continue;
                property.SetValue(copy, property.GetValue(original, null), null);
            }

            return copy as T;
        }
        public static T CopyTo<T>(this T original, T destination) where T : Component
        {
            Type type = original.GetType();
            if (type != destination.GetType()) return null;

            FieldInfo[] fields = type.GetFields();
            foreach (FieldInfo field in fields)
            {
                if (field.IsStatic) continue;
                field.SetValue(destination, field.GetValue(original));
            }

            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (!property.CanWrite || property.Name == "name" || property.IsDefined(typeof(ObsoleteAttribute), true)) continue;
                property.SetValue(destination, property.GetValue(original, null), null);
            }

            return destination as T;
        }

        public static float GetRandomInRange(this Vector2 vector2) => Random.Range(vector2.x, vector2.y);

        public static float GetMaxClipLength(this AudioSource source) => source.clip ? source.clip.length / Mathf.Abs(source.pitch) : 0f;
    }

    public abstract class Maf
    {
        public static Vector3 Direction(Vector3 from, Vector3 to) => (to - from).normalized;

        public static Vector3 Slerp(Vector3 from, Vector3 to, float t, Vector3 origin)
        {
            Vector3 dir1 = from - origin;
            Vector3 dir2 = to - origin;
            return origin + Vector3.Slerp(dir1, dir2, t);
        }

        public static float Average(params float[] nums)
        {
            float num = 0f;
            for (int i = 0; i < nums.Length; i++)
            {
                num += nums[i];
            }

            return num / nums.Length;
        }

        public static int RoundToNearest(float num) => (int)Math.Round(num, MidpointRounding.AwayFromZero);
        public static float RoundBy(float num, float roundBy) => roundBy * Mathf.Round(num / roundBy);
        public static float RoundToDecimalPlaces(float num, int decimalPlaces) => (float)Math.Round((decimal)num, decimalPlaces);

        public static float ReMap(float iMin, float iMax, float oMin, float oMax, float value)
        {
            float t = Mathf.InverseLerp(iMin, iMax, value);
            return Mathf.Lerp(oMin, oMax, t);
        }
        public static float FlipOne(float num)
        {
            return Mathf.Abs(1f - num);
        }

        public static Vector3 QuaternionToEuler(Quaternion quaternion)
        {
            return quaternion.eulerAngles;
        }
        public static Quaternion EulerToQuaternion(Vector3 euler) => Quaternion.Euler(euler);
        public static Quaternion EulerToQuaternion(float x, float y, float z) => Quaternion.Euler(x, y, z);

        public static bool RandomBool(int percentage) => Random.Range(0, 100) < percentage;
        public static bool RandomBool(float percentage) => Random.Range(0f, 1f) <= percentage * 0.01f;

        public static Quaternion GetRandomRotation() => Quaternion.Euler(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f));
        public static Vector3 GetRandomDirection() => Random.insideUnitSphere.normalized;

        public static Vector3 RandomPointInBounds(Bounds bounds) => bounds.center + new Vector3(Random.Range(-bounds.size.x * 0.5f, bounds.size.x * 0.5f), Random.Range(-bounds.size.y * 0.5f, bounds.size.y * 0.5f), Random.Range(-bounds.size.z * 0.5f, bounds.size.z * 0.5f));

        public static Vector3 CalculateCollisionPoint(Vector3 targetPos, Vector3 targetVelocity, Vector3 projectilePos, float projectileSpeed)
        {
            float timeToReachObject = CalculateTimeToCollision(targetPos - projectilePos, targetVelocity, projectileSpeed);

            if (timeToReachObject > 0f) return targetPos + targetVelocity * timeToReachObject;
            else return Vector3.zero;

            float CalculateTimeToCollision(Vector3 relativePosition, Vector3 relativeVelocity, float projectileSpeed)
            {
                float rVel = Vector3.Dot(relativeVelocity, relativeVelocity) - projectileSpeed * projectileSpeed;
                float rAvg = Vector3.Dot(relativeVelocity, relativePosition) * 2f;
                float rPos = Vector3.Dot(relativePosition, relativePosition);

                float disc = rAvg * rAvg - rVel * rPos * 4f;

                if (disc > 0f) return rPos * 2f / (Mathf.Sqrt(disc) - rAvg);
                else return -1f;
            }
        }
    }

    public static class Easing
    {
        // Everything here is very ugly, but it should work so who really cares. To visualize the functions see https://easings.net/.

        public enum EasingMode
        {
            Linear,
            EaseInSine,
            EaseOutSine,
            EaseInOutSine,
            EaseInQuad,
            EaseOutQuad,
            EaseInOutQuad,
            EaseInCubic,
            EaseOutCubic,
            EaseInOutCubic,
            EaseInQuart,
            EaseOutQuart,
            EaseInOutQuart,
            EaseInQuint,
            EaseOutQuint,
            EaseInOutQuint,
            EaseInCirc,
            EaseOutCirc,
            EaseInOutCirc,
            EaseInExpo,
            EaseOutExpo,
            EaseInOutExpo,
            EaseInBack,
            EaseOutBack,
            EaseInOutBack,
            EaseInElastic,
            EaseOutElastic,
            EaseInOutElastic,
            EaseInBounce,
            EaseOutBounce,
            EaseInOutBounce
        }


        const float c1 = 1.70158f;
        const float c2 = 2.5949095f;
        const float c3 = 2.70158f;
        const float c4 = 2 * Mathf.PI / 3;
        const float c5 = 2 * Mathf.PI / 4.5f;


        public static float SoftLimit(float x) => (float)Math.Tanh(x);

        public static float InSine(float t) => 1 - Mathf.Cos(t * Mathf.PI / 2);
        public static float OutSine(float t) => Mathf.Sin(t * Mathf.PI / 2);
        public static float InOutSine(float t) => -(Mathf.Cos(Mathf.PI * t) - 1) / 2;

        public static float InQuad(float t) => t * t;
        public static float OutQuad(float t) => 1 - (1 - t) * (1 - t);
        public static float InOutQuad(float t) => (t < 0.5f) ? (2 * t * t) : (1 - Mathf.Pow(-2 * t + 2, 2) / 2);

        public static float InCubic(float t) => t * t * t;
        public static float OutCubic(float t) => 1 - Mathf.Pow(1 - t, 3);
        public static float InOutCubic(float t) => (t > 0.5f) ? (4 * t * t * t) : (1 - Mathf.Pow(-2 * t + 2, 3) / 2);

        public static float InQuart(float t) => t * t * t * t;
        public static float OutQuart(float t) => 1 - Mathf.Pow(1 - t, 4);
        public static float InOutQuart(float t) => (t > 0.5f) ? (8 * t * t * t * t) : (1 - Mathf.Pow(-2 * t + 2, 4) / 2);

        public static float InQuint(float t) => t * t * t * t * t;
        public static float OutQuint(float t) => 1 - Mathf.Pow(1 - t, 5);
        public static float InOutQuint(float t) => (t > 0.5f) ? (16 * t * t * t * t * t) : (1 - Mathf.Pow(-2 * t + 2, 5) / 2);

        public static float InCirc(float t) => 1 - Mathf.Sqrt(1 - Mathf.Pow(t, 2));
        public static float OutCirc(float t) => Mathf.Sqrt(1 - Mathf.Pow(t - 1, 2));
        public static float InOutCirc(float t) => (t < 0.5f) ? ((1 - Mathf.Sqrt(1 - Mathf.Pow(2 * t, 2))) / 2) : ((Mathf.Sqrt(1 - Mathf.Pow(-2 * t + 2, 2)) + 1) / 2);

        public static float InExpo(float t) => (t == 0f) ? 0 : Mathf.Pow(2, 10 * t - 10);
        public static float OutExpo(float t) => (t == 1f) ? 1 : 1 - Mathf.Pow(2, -10 * t);
        public static float InOutExpo(float t) => (t == 0f) ? 0 : (t == 1f) ? 1 : (t < 0.5f) ? (Mathf.Pow(2, 20 * t - 10) / 2) : ((2 - Mathf.Pow(2, -20 * t + 10)) / 2);

        public static float InBack(float t) => c3 * t * t * t - c1 * t * t;
        public static float OutBack(float t) => 1 + c3 * Mathf.Pow(t - 1, 3) + c1 * Mathf.Pow(t - 1, 2);
        public static float InOutBack(float t) => (t < 0.5f) ? (Mathf.Pow(2 * t, 2) * ((c2 + 1) * 2 * t - c2) / 2) : ((Mathf.Pow(2 * t - 2, 2) * ((c2 + 1) * (t * 2 - 2) + c2) + 2) / 2);

        public static float InElastic(float t)
        {
            if (t == 0) return 0;
            else if (t == 1) return 1;
            else return -Mathf.Pow(2, 10 * t - 10) * Mathf.Sin((t * 10 - 10.75f) * c4);
        }
        public static float OutElastic(float t)
        {
            if (t == 0) return 0;
            else if (t == 1) return 1;
            else return Mathf.Pow(2, -10 * t) * Mathf.Sin((t * 10 - 0.75f) * c4) + 1;
        }
        public static float InOutElastic(float t)
        {
            if (t == 0) return 0;
            else if (t == 1) return 1;
            else if (t < 0.5f) return -(Mathf.Pow(2, 20 * t - 10) * Mathf.Sin((20 * t - 11.125f) * c5)) / 2;
            else return (Mathf.Pow(2, -20 * t + 10) * Mathf.Sin((20 * t - 11.125f) * c5)) / 2 + 1;
        }

        public static float InBounce(float t) => 1 - OutBounce(1 - t);
        public static float OutBounce(float t)
        {
            const float n1 = 7.5625f;
            const float d1 = 2.75f;

            if (t < 1 / d1) return n1 * t * t;
            else if (t < 2 / d1) return n1 * (t -= 1.5f / d1) * t + 0.75f;
            else if (t < 2.5 / d1) return n1 * (t -= 2.25f / d1) * t + 0.9375f;
            else return n1 * (t -= 2.625f / d1) * t + 0.984375f;
        }
        public static float InOutBounce(float t) => (t < 0.5) ? ((1 - OutBounce(1 - 2 * t)) / 2) : ((1 + OutBounce(2 * t - 1)) / 2);
    }

    public static class ControlRebinding
    {
        static Dictionary<string, KeyCode> bindings;

        static KeyCode[] validKeycodes;
        static KeyCode keyCode;

        public static bool binding { get; private set; }
        static string bindingKeyName;

        public static event Action OnKeyRebound;

        public static void SetValidKeycodes(bool includeController = false)
        {
            bindings = new Dictionary<string, KeyCode>();
            validKeycodes = Enum.GetValues(typeof(KeyCode))
                .Cast<KeyCode>()
                .Where(k => !includeController ? (int)k < 330 : true)
                .ToArray();
        }

        public static KeyCode GetPressedKey()
        {
            if (!Input.anyKeyDown) return KeyCode.None;

            for (int i = 0; i < validKeycodes.Length; i++)
            {
                keyCode = validKeycodes[i];
                if (Input.GetKeyDown(keyCode)) return keyCode;
            }

            return KeyCode.None;
        }

        public static KeyCode GetKeyWithName(string name)
        {
            return bindings[name];
        }

        public static void StartBinding(string name)
        {
            binding = true;
            bindingKeyName = name;
        }

        public static void HandleBinding()
        {
            KeyCode key = GetPressedKey();

            if (key != KeyCode.None)
            {
                EditBinding(bindingKeyName, key);
                binding = false;
            }
        }

        public static void EditBinding(string name, KeyCode key)
        {
            bindings[name] = key;
            OnKeyRebound?.Invoke();
        }

        public static void SaveBindings()
        {
            Serializer.SaveGlobal(bindings, "ControlBindings");
        }

        public static void LoadBindings()
        {
            if (Serializer.LoadGlobal(out bindings, "ControlBindings"))
            {
                OnKeyRebound?.Invoke();
            }
        }
    }

    public static class Regexer
    {
        public static RegexObject Begin()
        {
            return new RegexObject();
        }
        public static string Finish(this RegexObject regex)
        {
            return regex.sb.ToString();
        }

        public class RegexObject
        {
            public StringBuilder sb;

            public RegexObject()
            {
                sb = new StringBuilder();
            }

            public RegexObject Any()
            {
                sb.Append(".");
                return AnyTimes();
            }
            public RegexObject Any(int count)
            {
                sb.Append(".");
                return Times(count);
            }
            public RegexObject Any(int min, int max)
            {
                sb.Append($".");
                return MinMaxTimes(min, max);
            }

            public RegexObject Exact(string text)
            {
                text = text.AddInFrontOfMatches("\\", @"\", @"^", @"$", @".", @"|", @"?", @"*", @"+", @"(", @")", @"[", @"]", @"{", @"}");
                sb.Append($"{text}");
                return this;
            }

            public RegexObject UppercaseCharacter()
            {
                sb.Append("[A-Z]");
                return AnyTimes();
            }
            public RegexObject UppercaseCharacter(int count)
            {
                sb.Append("[A-Z]");
                return Times(count);
            }
            public RegexObject UppercaseCharacter(int min, int max)
            {
                sb.Append("[A-Z]");
                return MinMaxTimes(min, max);
            }

            public RegexObject LowercaseCharacter()
            {
                sb.Append("[a-z]");
                return AnyTimes();
            }
            public RegexObject LowercaseCharacter(int count)
            {
                sb.Append("[a-z]");
                return Times(count);
            }
            public RegexObject LowercaseCharacter(int min, int max)
            {
                sb.Append("[a-z]");
                return MinMaxTimes(min, max);
            }

            public RegexObject UppercaseOrLowercaseCharacter()
            {
                sb.Append("[a-zA-Z]");
                return AnyTimes();
            }
            public RegexObject UppercaseOrLowercaseCharacter(int count)
            {
                sb.Append("[a-zA-Z]");
                return Times(count);
            }
            public RegexObject UppercaseOrLowercaseCharacter(int min, int max)
            {
                sb.Append("[a-zA-Z]");
                return MinMaxTimes(min, max);
            }

            public RegexObject Number()
            {
                sb.Append("[0-9]");
                return AnyTimes();
            }
            public RegexObject Number(int count)
            {
                sb.Append("[0-9]");
                return Times(count);
            }
            public RegexObject Number(int min, int max)
            {
                sb.Append("[0-9]");
                return MinMaxTimes(min, max);
            }

            public RegexObject NonSymbolCharacter()
            {
                sb.Append("[a-zA-Z0-9]");
                return AnyTimes();
            }
            public RegexObject NonSymbolCharacter(int count)
            {
                sb.Append("[a-zA-Z0-9]");
                return Times(count);
            }
            public RegexObject NonSymbolCharacter(int min, int max)
            {
                sb.Append("[a-zA-Z0-9]");
                return MinMaxTimes(min, max);
            }

            public RegexObject Custom(string custom)
            {
                sb.Append(custom);
                return this;
            }

            public RegexObject AnyTimes()
            {
                sb.Append("*");
                return this;
            }
            public RegexObject Times(int count)
            {
                sb.Append($@"{{{count}}}");
                return this;
            }
            public RegexObject MinMaxTimes(int min, int max)
            {
                sb.Append($@"{{{min},{max}}}");
                return this;
            }
            public RegexObject MinTimes(int count)
            {
                sb.Append($@"{{{count},}}");
                return this;
            }

            public RegexObject Start()
            {
                sb.Append("^");
                return this;
            }
            public RegexObject End()
            {
                sb.Append("$");
                return this;
            }


            public override string ToString() => this.Finish();

            public static implicit operator string (RegexObject regex) => regex.Finish();
        }
    }

    public static class SpringUtils
    {
        static DampedSpringMotionParams motionParams = new DampedSpringMotionParams();

        class DampedSpringMotionParams
        {
            public float posPosCoef, posVelCoef;
            public float velPosCoef, velVelCoef;
        };


        public static void CalculateHarmonicSpringMotion(ref float position, ref float velocity, float goalPos, float frequency = 3.5f, float damping = 0.5f, float deltaTime = float.Epsilon)
        {
            if (deltaTime == float.Epsilon) deltaTime = Time.deltaTime;

            CalculateDampedSpringMotionParams(ref motionParams, deltaTime, frequency, damping);
            UpdateDampedSpringMotion(ref position, ref velocity, goalPos, motionParams);
        }

        static void CalculateDampedSpringMotionParams(ref DampedSpringMotionParams springParams, float deltaTime, float angularFrequency, float dampingRatio)
        {
            if (dampingRatio < 0.0f) dampingRatio = 0.0f;
            if (angularFrequency < 0.0f) angularFrequency = 0.0f;

            if (angularFrequency < float.Epsilon)
            {
                springParams.posPosCoef = 1.0f; springParams.posVelCoef = 0.0f;
                springParams.velPosCoef = 0.0f; springParams.velVelCoef = 1.0f;
                return;
            }


            if (dampingRatio > 1.0f + float.Epsilon)
            {
                // over-damped
                float za = -angularFrequency * dampingRatio;
                float zb = angularFrequency * Mathf.Sqrt(dampingRatio * dampingRatio - 1f);
                float z1 = za - zb;
                float z2 = za + zb;

                float e1 = Mathf.Exp(z1 * deltaTime);
                float e2 = Mathf.Exp(z2 * deltaTime);

                float invTwoZb = 1f / (2f * zb);

                float e1_Over_TwoZb = e1 * invTwoZb;
                float e2_Over_TwoZb = e2 * invTwoZb;

                float z1e1_Over_TwoZb = z1 * e1_Over_TwoZb;
                float z2e2_Over_TwoZb = z2 * e2_Over_TwoZb;

                springParams.posPosCoef = e1_Over_TwoZb * z2 - z2e2_Over_TwoZb + e2;
                springParams.posVelCoef = -e1_Over_TwoZb + e2_Over_TwoZb;

                springParams.velPosCoef = (z1e1_Over_TwoZb - z2e2_Over_TwoZb + e2) * z2;
                springParams.velVelCoef = -z1e1_Over_TwoZb + z2e2_Over_TwoZb;
            }
            else if (dampingRatio < 1f - float.Epsilon)
            {
                // under-damped
                float omegaZeta = angularFrequency * dampingRatio;
                float alpha = angularFrequency * Mathf.Sqrt(1f - dampingRatio * dampingRatio);

                float expTerm = Mathf.Exp(-omegaZeta * deltaTime);
                float cosTerm = Mathf.Cos(alpha * deltaTime);
                float sinTerm = Mathf.Sin(alpha * deltaTime);

                float invAlpha = 1f / alpha;

                float expSin = expTerm * sinTerm;
                float expCos = expTerm * cosTerm;
                float expOmegaZetaSin_Over_Alpha = expTerm * omegaZeta * sinTerm * invAlpha;

                springParams.posPosCoef = expCos + expOmegaZetaSin_Over_Alpha;
                springParams.posVelCoef = expSin * invAlpha;

                springParams.velPosCoef = -expSin * alpha - omegaZeta * expOmegaZetaSin_Over_Alpha;
                springParams.velVelCoef = expCos - expOmegaZetaSin_Over_Alpha;
            }
            else
            {
                // critically damped
                float expTerm = Mathf.Exp(-angularFrequency * deltaTime);
                float timeExp = deltaTime * expTerm;
                float timeExpFreq = timeExp * angularFrequency;

                springParams.posPosCoef = timeExpFreq + expTerm;
                springParams.posVelCoef = timeExp;

                springParams.velPosCoef = -angularFrequency * timeExpFreq;
                springParams.velVelCoef = -timeExpFreq + expTerm;
            }
        }
        static void UpdateDampedSpringMotion(ref float pPos, ref float pVel, float equilibriumPos, in DampedSpringMotionParams springParams)
        {
            float oldPos = pPos - equilibriumPos;
            float oldVel = pVel;

            pPos = oldPos * springParams.posPosCoef + oldVel * springParams.posVelCoef + equilibriumPos;
            pVel = oldPos * springParams.velPosCoef + oldVel * springParams.velVelCoef;
        }
    }

    [Serializable]
    public class Spring
    {
        [SerializeField] float frequency = 3f;
        [SerializeField][Range(0f, 1f)] float damping = 0.5f;
        [SerializeField] float goal = 1f;
        [field: SerializeField] public float Min { get; private set; } = -Mathf.Infinity;
        [field: SerializeField] public float Max { get; private set; } = Mathf.Infinity;

        public float Velocity => velocity;
        float velocity;
        float pos;


        Spring() { } // Stupid workaround so Unity properly serializes the class with the correct default values

        public Spring(float frequency, float damping, float start = 0f)
        {
            this.frequency = frequency;
            this.damping = damping;

            pos = start;
            goal = start;
        }


        public void Update(float deltaTime = float.Epsilon)
        {
            SpringUtils.CalculateHarmonicSpringMotion(ref pos, ref velocity, goal, frequency, damping, deltaTime);
            pos = Mathf.Clamp(pos, Min, Max);
        }

        public float GetValue()
        {
            Update();
            return pos;
        }
        public float GetValueWithoutUpdate() => pos;

        public void SetGoal(float goal) => this.goal = Mathf.Clamp(goal, Min, Max);

        public void AddForce(float force) => velocity += force;
        public void SetVelocity(float velocity) => this.velocity = velocity;
        public void SetValue(float pos) => this.pos = pos;
        public void SetMinMax(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }


    namespace Ticker
    {
        [DefaultExecutionOrder(-50)]
        class HKTickCaller : MonoBehaviour
        {
            void Update() => HKTicker.UpdateTickers(Time.deltaTime);
        }

        class TickerCollection
        {
            public HKTicker[] Tickers { get; private set; }
            float max;
            int index;

            internal TickerCollection(float max, int batches)
            {
                batches = Mathf.Max(1, batches); // Min 1 batch to not cause divide by 0 error
                float timeOffset = 1f / batches; // 5 batch -> 1 / 5 -> 0.2 -> 0, 0.2, 0.4, 0.6, 0.8
                this.max = max;

                Tickers = new HKTicker[batches];
                for (int i = 0; i < batches; i++)
                {
                    Tickers[i] = new HKTicker(i * timeOffset * max);
                }
            }

            public void Update(float deltaTime)
            {
                for (int i = 0; i < Tickers.Length; i++)
                {
                    Tickers[i].Update(deltaTime, max);
                }
            }

            public HKTicker GetTicker()
            {
                HKTicker ticker = Tickers[index];
                index++;
                index %= Tickers.Length;
                return ticker;
            }
        }

        public class HKTicker
        {
            static Dictionary<float, TickerCollection> tickers = new Dictionary<float, TickerCollection>();
            public static int Batches = 3;


            internal HKTicker(float time) => this.time = time;


            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
            static void Initialize()
            {
                tickers.Clear();
                //Debug.Log($"Cleared tickers.");

                GameObject tickCaller = new GameObject("[HK Ticker]", typeof(HKTickCaller));
                GameObject.DontDestroyOnLoad(tickCaller);
                //Debug.Log($"Created tick caller.");
            }


            public static void UpdateTickers(float deltaTime)
            {
                foreach (TickerCollection ticker in tickers.Values)
                {
                    ticker.Update(deltaTime);
                }
            }

            public static HKTicker GetTicker(float delay)
            {
                if (tickers.TryGetValue(delay, out TickerCollection ticker)) return ticker.GetTicker();
                else
                {
                    TickerCollection tickerCollection = new TickerCollection(delay, Batches);
                    tickers[delay] = tickerCollection;
                    return tickerCollection.GetTicker();
                }
            }


            public event Action OnTick;
            float time;

            public void Update(float deltaTime, float max)
            {
                time += deltaTime;
                if (time >= max)
                {
                    time -= max;
                    OnTick?.Invoke();
                }
            }
        }
    }

    namespace Serialization
    {
        using System.IO;

        public static class Serializer
        {
            public static string SAVEDATA_FOLDER = Path.Combine(Application.persistentDataPath, "SaveData");
            const string SAVE_FOLDER_TEMPLATE = "Save_";
            const string FILE_EXTENSION = ".SAVE";

            const string EMPTY_STRING = "";

            public static SerializationType SerializationType = SerializationType.JSONFormatted;
            public static int CurrentSaveID = 1;


            #region Saving
            public static void SaveGlobal<TSaveType>(TSaveType saveData, string saveKey, string subFolder = EMPTY_STRING)
            {
                switch (SerializationType)
                {
                    case SerializationType.Binary:
                        SaveToDisk(SerializeToBytes(saveData), saveKey, GetGlobalSaveDirectory(subFolder));
                        break;

                    case SerializationType.JSON:
                        SaveJSONToDisk(SerializeObject(saveData, Formatting.None), saveKey, GetGlobalSaveDirectory(subFolder));
                        break;

                    case SerializationType.JSONFormatted:
                        SaveJSONToDisk(SerializeObject(saveData, Formatting.Indented), saveKey, GetGlobalSaveDirectory(subFolder));
                        break;
                }
            }
            public static void SaveGlobal(byte[] saveData, string saveKey, string subFolder = EMPTY_STRING) => SaveToDisk(saveData, saveKey, GetGlobalSaveDirectory(subFolder));

            public static void Save<TSaveType>(TSaveType saveData, string saveKey, string subFolder = EMPTY_STRING)
            {
                switch (SerializationType)
                {
                    case SerializationType.Binary:
                        SaveToDisk(SerializeToBytes(saveData), saveKey, GetSaveDirectory(subFolder));
                        break;

                    case SerializationType.JSON:
                        SaveJSONToDisk(SerializeObject(saveData, Formatting.None), saveKey, GetSaveDirectory(subFolder));
                        break;

                    case SerializationType.JSONFormatted:
                        SaveJSONToDisk(SerializeObject(saveData, Formatting.Indented), saveKey, GetSaveDirectory(subFolder));
                        break;
                }
            }
            public static void Save(byte[] saveData, string saveKey, string subFolder = EMPTY_STRING) => SaveToDisk(saveData, saveKey, GetSaveDirectory(subFolder));

            static void SaveToDisk(byte[] saveData, string saveKey, string path)
            {
                CreateDirectoryIfNeeded(path);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Compress))
                    {
                        gzipStream.Write(saveData, 0, saveData.Length);
                    }

                    File.WriteAllBytes(GetTotalPath(saveKey, path), memoryStream.ToArray());
                }
            }
            static void SaveJSONToDisk(string JSON, string saveKey, string path)
            {
                CreateDirectoryIfNeeded(path);
                File.WriteAllText(GetTotalPath(saveKey, path), JSON);
            }

            static byte[] SerializeToBytes<TType>(TType data) => System.Text.Encoding.UTF8.GetBytes(SerializeObject(data, Formatting.None));
            static string SerializeObject<TType>(TType data, Formatting formatting) => JsonConvert.SerializeObject(data, formatting);

            static void CreateDirectoryIfNeeded(string path)
            {
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            }
            #endregion
            #region Loading
            public static bool LoadGlobal<TLoadType>(out TLoadType loadData, string loadKey, string subFolder = EMPTY_STRING)
            {
                return LoadFromDisk(out loadData, loadKey, GetGlobalSaveDirectory(subFolder));
            }
            public static bool LoadGlobal(out byte[] loadData, string loadKey, string subFolder = EMPTY_STRING)
            {
                return LoadFromDisk(out loadData, loadKey, GetGlobalSaveDirectory(subFolder));
            }

            public static bool Load<TLoadType>(out TLoadType loadData, string loadKey, string subFolder = EMPTY_STRING)
            {
                return LoadFromDisk(out loadData, loadKey, GetSaveDirectory(subFolder));
            }
            public static bool Load(out byte[] loadData, string loadKey, string subFolder = EMPTY_STRING)
            {
                return LoadFromDisk(out loadData, loadKey, GetSaveDirectory(subFolder));
            }

            static bool LoadFromDisk<TLoadType>(out TLoadType loadData, string loadKey, string path)
            {
                switch (SerializationType)
                {
                    default:
                        loadData = default;
                        return false;

                    case SerializationType.Binary:

                        if (LoadFromDisk(out byte[] compressedData, loadKey, path))
                        {
                            using (MemoryStream memoryStream = new MemoryStream(compressedData))
                            {
                                using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                                {
                                    using (StreamReader reader = new StreamReader(gzipStream))
                                    {
                                        loadData = JsonConvert.DeserializeObject<TLoadType>(reader.ReadToEnd());
                                        return true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            loadData = default;
                            return false;
                        }

                    case SerializationType.JSON:

                        if (File.Exists(GetTotalPath(loadKey, path)))
                        {
                            string JSON = File.ReadAllText(GetTotalPath(loadKey, path));
                            loadData = JsonConvert.DeserializeObject<TLoadType>(JSON);
                            return true;
                        }
                        else
                        {
                            loadData = default;
                            return false;
                        }

                    case SerializationType.JSONFormatted:

                        if (File.Exists(GetTotalPath(loadKey, path)))
                        {
                            string JSON = File.ReadAllText(GetTotalPath(loadKey, path));
                            loadData = JsonConvert.DeserializeObject<TLoadType>(JSON);
                            return true;
                        }
                        else
                        {
                            loadData = default;
                            return false;
                        }
                }
            }
            static bool LoadFromDisk(out byte[] loadData, string loadKey, string path)
            {
                string totalPath = GetTotalPath(loadKey, path);
                if (File.Exists(totalPath))
                {
                    loadData = File.ReadAllBytes(totalPath);
                    return true;
                }
                else
                {
                    loadData = new byte[0];
                    return false;
                }
            }
            #endregion
            #region Utilities
            static string GetSaveDirectory(string subFolder)
            {
                subFolder = subFolder.Replace('/', Path.DirectorySeparatorChar);
                return Path.Combine(SAVEDATA_FOLDER, $"{SAVE_FOLDER_TEMPLATE}{CurrentSaveID}", subFolder);
            }
            static string GetGlobalSaveDirectory(string subFolder)
            {
                subFolder = subFolder.Replace('/', Path.DirectorySeparatorChar);
                return Path.Combine(SAVEDATA_FOLDER, "Global", subFolder);
            }
            static string GetFileName(string saveKey) => $"{saveKey}{FILE_EXTENSION}";
            static string GetTotalPath(string key, string path) => Path.Combine(path, GetFileName(key));

            static int GetSaveIDFromDirectory(string directory) => int.Parse(directory.Remove(Path.Combine(SAVEDATA_FOLDER, SAVE_FOLDER_TEMPLATE)));
            static string GetDirectoryFromSaveID(int saveID) => $"{Path.Combine(SAVEDATA_FOLDER, SAVE_FOLDER_TEMPLATE)}{saveID}";
            #endregion
            #region Helper Methods
            public static int[] GetSaveIDs()
            {
                string[] saveDirectories = Directory.GetDirectories(SAVEDATA_FOLDER);
                int saveDirectoryCount = saveDirectories.Length;

                int[] saveIDs = new int[saveDirectoryCount];

                for (int i = 0; i < saveDirectoryCount; i++)
                {
                    saveIDs[i] = GetSaveIDFromDirectory(saveDirectories[i]);
                }
                return saveIDs;
            }
            public static int GetHighestAvailableSaveID()
            {
                int saveID = 1;

                while (Directory.Exists(GetDirectoryFromSaveID(saveID))) saveID++;
                return saveID;
            }

            public static void ClearSave(int saveID = -1)
            {
                if (saveID == -1) saveID = CurrentSaveID;

                string directory = GetDirectoryFromSaveID(saveID);
                if (Directory.Exists(directory)) Directory.Delete(directory, true);
            }

            public static void ClearGlobalSaveData(string saveKey, string subFolder)
            {
                string filePath = GetTotalPath(saveKey, GetGlobalSaveDirectory(subFolder));
                if (File.Exists(filePath)) File.Delete(filePath);
            }
            public static void ClearSaveData(string saveKey, string subFolder)
            {
                string filePath = GetTotalPath(saveKey, GetSaveDirectory(subFolder));
                if (File.Exists(filePath)) File.Delete(filePath);
            }
            #endregion
        }

        public enum SerializationType
        {
            Binary,
            JSON,
            JSONFormatted
        }
    }

    namespace Commands
    {
        using UnityEngine.Scripting;

        public static class CommandSystem
        {
            public static List<DebugCommandBase> commandList = new List<DebugCommandBase>();

            public static void AddCommand(DebugCommandBase command)
            {
                if (command.commandName.Contains(" ")) command.commandName.Replace(" ", "_");
                commandList.Add(command);
            }
        }

        public abstract class DebugCommandBase : IComparable<DebugCommandBase>
        {
            public string commandName;

            public Type[] types;

            public Delegate CommandDelegate;
            public bool hidden;

            public DebugCommandBase(string commandName, bool hidden = false)
            {
                this.commandName = commandName;
                this.hidden = hidden;
            }

            public abstract bool TryExecute(int parameterCount, string[] properties);

            public int CompareTo(DebugCommandBase other)
            {
                return string.Compare(commandName, other.commandName);
            }
        }

        public class DebugCommand : DebugCommandBase
        {
            Action command;

            public DebugCommand(string commandName, Action command, bool hidden = false) : base(commandName, hidden)
            {
                this.command = command;
                types = new Type[0];
            }

            public void Execute()
            {
                command.Invoke();
            }

            public override bool TryExecute(int parametercount, string[] properties)
            {
                command.Invoke();
                return true;
            }
        }

        public class DebugCommand<T1> : DebugCommandBase
        {
            Action<T1> command;

            public DebugCommand(string commandName, Action<T1> command, bool hidden = false) : base(commandName, hidden)
            {
                this.command = command;
                types = new Type[] { typeof(T1) };
            }

            public void Execute(T1 value)
            {
                command.Invoke(value);
            }

            public override bool TryExecute(int parameterCount, string[] properties)
            {
                if (parameterCount != types.Length) return false;

                Execute((T1)Convert.ChangeType(properties[1], typeof(T1)));
                return true;
            }
        }

        public class DebugCommand<T1, T2> : DebugCommandBase
        {
            Action<T1, T2> command;

            public DebugCommand(string commandName, Action<T1, T2> command, bool hidden = false) : base(commandName, hidden)
            {
                this.command = command;
                types = new Type[] { typeof(T1), typeof(T2) };
            }

            public void Execute(T1 value, T2 value2)
            {
                command.Invoke(value, value2);
            }

            public override bool TryExecute(int parameterCount, string[] properties)
            {
                if (parameterCount != types.Length) return false;

                Execute((T1)Convert.ChangeType(properties[1], typeof(T1)), (T2)Convert.ChangeType(properties[2], typeof(T2)));
                return true;
            }
        }


        [AttributeUsage(AttributeTargets.Method)]
        public class HKCommandAttribute : PreserveAttribute // Derive from Preserve to make sure the methods are never stripped by Code Stripping
        {

        }
    }

    namespace QOL
    {
        public static class QOL
        {
            public const float Tau = Mathf.PI * 2;

            public static bool Raycast2D(Vector2 origin, Vector2 direction, out RaycastHit2D hit, float distance)
            {
                return hit = Physics2D.Raycast(origin, direction, distance);
            }
            public static bool Raycast2D(Vector2 origin, Vector2 direction, out RaycastHit2D hit, float distance, int layerMask)
            {
                return hit = Physics2D.Raycast(origin, direction, distance, layerMask);
            }

            public static void CopyToClipboard(string text)
            {
                GUIUtility.systemCopyBuffer = text;
            }


            static Dictionary<float, WaitForSeconds> waitDictionary = new Dictionary<float, WaitForSeconds>();
            static WaitForSeconds waitCache;
            public static WaitForSeconds GetWaitForSeconds(float time)
            {
                if (waitDictionary.TryGetValue(time, out waitCache)) return waitCache;
                return waitDictionary[time] = new WaitForSeconds(time);
            }
            public static void PurgeWaitCache() => waitDictionary.Clear();

            static Dictionary<float, WaitForSecondsRealtime> unscaledWaitDictionary = new Dictionary<float, WaitForSecondsRealtime>();
            static WaitForSecondsRealtime unscaledWaitCache;
            public static WaitForSecondsRealtime GetUnscaledWaitForSeconds(float time)
            {
                if (unscaledWaitDictionary.TryGetValue(time, out unscaledWaitCache)) return unscaledWaitCache;
                return unscaledWaitDictionary[time] = new WaitForSecondsRealtime(time);
            }
            public static void PurgeUnscaledWaitCache() => unscaledWaitDictionary.Clear();

            public static void Destroy(GameObject go)
            {
#if UNITY_EDITOR
                if (Application.isPlaying) GameObject.Destroy(go);
                else GameObject.DestroyImmediate(go);
#else
                GameObject.Destroy(go);
#endif
            }
            public static void Quit()
            {
#if UNITY_EDITOR
                EditorApplication.ExitPlaymode();
#else
                Application.Quit();
#endif
            }

#if !UNITY_EDITOR && HK_LOG_OFF
[System.Diagnostics.Conditional("c5ae3fäf7d9ö9f")]
#endif
            public static void Log(object message) => Debug.Log(message); // Only compiled in the Editor, or if the 'HK_LOG_OFF' flag is NOT set. i.e. Opt out for builds
        }

        [Flags]
        public enum Direction
        {
            None = 0,
            Up = 1,
            Down = 2,
            Left = 4,
            Right = 8,
            Forward = 16,
            Back = 32
        }

        public enum VariableType
        {
            None = 0,
            String,
            Float,
            Enum,
            Bool,
            Int
        }
    }
}