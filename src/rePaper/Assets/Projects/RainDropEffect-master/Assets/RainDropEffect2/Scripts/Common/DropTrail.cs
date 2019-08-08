using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if !UNITY_5_5_OR_NEWER
public enum LineTextureMode
{
    Stretch,
    Tile,
}
#endif

public class DropTrail : MonoBehaviour
{
    [System.Serializable]
    class Path
    {
        public float timeCreated = 0;
        public float timeElapsed
        {
            get { return Time.time - timeCreated; }
        }
        public float fadeAlpha = 0;
        public Vector3 localPosition = Vector3.zero;
        public Quaternion localRotation = Quaternion.identity;
        public Path(Vector3 localPosition, Quaternion localRotation)
        {
            this.localPosition = localPosition;
            this.localRotation = localRotation;
            timeCreated = Time.time;
        }
    }

	public bool enabled = true;
    public Material material;
    public float lifeTime = 3f;
	public AnimationCurve widthCurve;
	public float widthMultiplier = .5f;
	public int angleDivisions = 10;
	public float vertexDistance = .5f;
    public LineTextureMode textureMode;


	const string _name = "[Hidden]DropTrailMesh";

	[HideInInspector]
    GameObject _trail;

	[HideInInspector]
    Vector3 _relativePos;

	[HideInInspector]
    MeshFilter _meshFilter;

	[HideInInspector]
    MeshRenderer _meshRenderer;

	Mesh _mesh
	{
		get { return _meshFilter.mesh; }
		set { _meshFilter.mesh = value; }
	}

	[SerializeField]
    List<Path> paths = new List<Path>();
    int pathCnt { 
		get { return paths.Count(); } 
	}


    // Use this for initialization
    void Awake()
    {
		CheckExistence();
    }


    // Update is called once per frame
    void Update()
    {
		if (!CheckExistence())
			return;

		if (!CheckActive ()) 
			return;

        UpdateTrail();
        UpdateMesh();
    }


	public void Clear()
	{
		paths.Clear ();
	}


	bool CheckExistence()
	{
		if (!_trail) {
			Transform oldTrail = transform.Find (_name);
			if (oldTrail)
			{
				_trail = oldTrail.gameObject;
				_meshFilter = _trail.GetComponent<MeshFilter>();
				_meshRenderer = _trail.GetComponent<MeshRenderer>();
			} 
			else 
			{
				_trail = RainDropTools.CreateHiddenObject (_name, this.transform).gameObject;
			}
		}

		if (!_meshFilter) 
		{
			_meshFilter = _trail.AddComponent<MeshFilter>();
		}

		if (!_meshRenderer) 
		{
			_meshRenderer = _trail.AddComponent<MeshRenderer>();
		}

		if (material == null) 
		{
			return false;
		}
		else
		{
			_meshRenderer.material = material;
		}

		return true;
	}


	bool CheckActive()
	{
		_meshRenderer.enabled = enabled;
		return enabled;
	}


    void UpdateTrail()
    {
        // Remove all expireds
        paths.RemoveAll(t => t.timeElapsed >= lifeTime);

        // Add paths
        if (pathCnt == 0)
        {
            paths.Add(new Path(transform.localPosition, transform.localRotation));
            paths.Add(new Path(transform.localPosition, transform.localRotation));
            _relativePos = transform.localPosition;
        }

		if (pathCnt == 1) {
			paths.Add(new Path(transform.localPosition, transform.localRotation));
			_relativePos = transform.localPosition;
		}

        // Add if needed
        float distSqr = (paths[0].localPosition - this.transform.localPosition).sqrMagnitude;
		if (distSqr < vertexDistance) 
		{
			return;
		}

        Vector3 vec1 = paths[0].localPosition - paths[1].localPosition;
        Vector3 vec2 = transform.localPosition - paths[0].localPosition;

		Quaternion qv1 = Quaternion.identity;
		Quaternion qv2 = Quaternion.identity;

		if(vec1.magnitude != 0f)
			qv1 = Quaternion.LookRotation(vec1, Vector3.forward);
		if(vec2.magnitude != 0f)
			qv2 = Quaternion.LookRotation(vec2, Vector3.forward);

		qv1.eulerAngles += Vector3.forward * -90f;
		qv2.eulerAngles += Vector3.forward * -90f;

        if (paths.Count() >= 2)
        {
            //Get the dot product
            float dot = Vector3.Dot(vec1, vec2);
            dot = dot / (vec1.magnitude * vec2.magnitude);
            float acos = Mathf.Acos(dot);
            float angle = acos * 180f / Mathf.PI;
            if (!float.IsNaN(angle))
            {
				int angleResol = (int)angle / angleDivisions;
                for (int j = 0; j < angleResol; j++)
                {
                    Quaternion q = Quaternion.Slerp(qv1, qv2, j / (float)angleResol);
                    paths.Insert(0, new Path(paths[0].localPosition, q));
                }
            }
        }
        _relativePos = vec2;
        paths.Insert(0, new Path(transform.localPosition, qv2));
    }


    void UpdateMesh()
    {
		if (pathCnt <= 1) 
		{
			_meshRenderer.enabled = false;
			return;
		}

		_meshRenderer.enabled = true;

        Vector3[] verts = new Vector3[pathCnt * 2];
        Vector2[] uvs = new Vector2[pathCnt * 2];
        int[] tris = new int[(pathCnt - 1) * 6];

        for (int i = 0; i < pathCnt; i++)
        {
			float progress = i / (float)pathCnt;
            Path p = paths[i];
            _trail.transform.parent = this.transform.parent;
            _trail.transform.localPosition = p.localPosition;
            _trail.transform.localRotation = p.localRotation;

			float w = Mathf.Max(widthMultiplier * widthCurve.Evaluate(progress) * 0.5f, 0.001f);
            verts[i * 2] = _trail.transform.TransformPoint(0, w, 0);
            verts[(i * 2) + 1] = _trail.transform.TransformPoint(0, -w, 0);

			float uvRatio = progress;
			if (textureMode == LineTextureMode.Tile)
			{
				uvRatio = i;
			}
            uvs[i * 2] = new Vector2(uvRatio, 0f);
            uvs[(i * 2) + 1] = new Vector2(uvRatio, 1f);

            if (i != 0)
            {
                tris[((i - 1) * 6) + 0] = (i * 2) - 2;
                tris[((i - 1) * 6) + 1] = (i * 2) - 1;
                tris[((i - 1) * 6) + 2] = (i * 2) - 0;
                tris[((i - 1) * 6) + 3] = (i * 2) + 1;
                tris[((i - 1) * 6) + 4] = (i * 2) + 0;
                tris[((i - 1) * 6) + 5] = (i * 2) - 1;
            }

            _trail.transform.parent = null;
        }

        _mesh.Clear();
        _mesh.vertices = verts;
        _mesh.uv = uvs;
        _mesh.triangles = tris;

        _trail.transform.localPosition = Vector3.zero;
        _trail.transform.localRotation = Quaternion.identity;
        _trail.transform.localScale = Vector3.one;

        _trail.transform.parent = this.transform;
    }


    void OnDrawGizmos()
    {
        if(_relativePos != Vector3.zero)
        {
            Vector3 fwd1 = transform.TransformPoint(0f, 0f, 0f);
            Vector3 fwd2 = transform.TransformPoint(_relativePos);
            Vector3 fwd = fwd2 - fwd1;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(
                transform.position,
                transform.position + fwd.normalized * 2f
                );
        }
    }
}
