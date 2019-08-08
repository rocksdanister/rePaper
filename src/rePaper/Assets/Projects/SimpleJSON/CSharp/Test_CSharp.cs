using UnityEngine;
using SimpleJSON;


class Test_CSharp : MonoBehaviour
{

	private string m_InGameLog = "";
	private Vector2 m_Position = Vector2.zero;
    void P(string aText)
    {
		m_InGameLog += aText + "\n";
    }
	
    void Test()
    {
        var N = JSONNode.Parse("{\"name\":\"test\", \"array\":[1,{\"data\":\"value\"}]}");
        N["array"][1]["Foo"] = "Bar";
        P("'nice formatted' string representation of the JSON tree:");
        P(N.ToString(""));
        P("");

        P("'normal' string representation of the JSON tree:");
        P(N.ToString());
        P("");

        P("content of member 'name':");
        P(N["name"]);
        P("");

        P("content of member 'array':");
        P(N["array"].ToString(""));
        P("");

        P("first element of member 'array': " + N["array"][0]);
        P("");

        N["array"][0].AsInt = 10;
        P("value of the first element set to: " + N["array"][0]);
        P("The value of the first element as integer: " + N["array"][0].AsInt);
        P("");

        P("N[\"array\"][1][\"data\"] == " + N["array"][1]["data"]);
        P("");

        var data = N.SaveToBase64();
        var data2 = N.SaveToCompressedBase64();
        N = null;
        P("Serialized to Base64 string:");
        P(data);
        P("Serialized to Base64 string (compressed):");
        P(data2);
        P("");

        N = JSONNode.LoadFromBase64(data);
        P("Deserialized from Base64 string:");
        P(N.ToString());
        P("");

        var I = new JSONClass();
        I["version"].AsInt = 5;
        I["author"]["name"] = "Bunny83";
        I["author"]["phone"] = "0123456789";
        I["data"][-1] = "First item\twith tab";
        I["data"][-1] = "Second item";
        I["data"][-1]["value"] = "class item";
        I["data"].Add("Forth item");
        I["data"][1] = I["data"][1] + " 'addition to the second item'";
        I.Add("version", "1.0");

        P("Second example:");
        P(I.ToString());
        P("");

        P("I[\"data\"][0]            : " + I["data"][0]);
        P("I[\"data\"][0].ToString() : " + I["data"][0].ToString());
        P("I[\"data\"][0].Value      : " + I["data"][0].Value);
		P (I.ToString());
    }

    void Start()
    {
        Test();
		Debug.Log("Test results:\n" + m_InGameLog);
    }
	void OnGUI()
	{
		m_Position = GUILayout.BeginScrollView(m_Position);
		GUILayout.Label(m_InGameLog);
		GUILayout.EndScrollView();
	}
}