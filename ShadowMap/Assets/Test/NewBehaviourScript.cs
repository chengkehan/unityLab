using UnityEngine;
using System.Collections;

public class NewBehaviourScript : MonoBehaviour {

    public Camera light = null;

    public Texture shadowMap = null;

    public float shadowDist = 0.03f;

    public Transform cube = null;

    public Camera mainCam = null;

    void Start()
    {
        
    }

	void OnPreRender () 
    {
        //Shader.SetGlobalTexture("_x_shadowMap", shadowMap);
        Shader.SetGlobalMatrix("_x_lightVP", light.projectionMatrix * light.worldToCameraMatrix);
        Shader.SetGlobalTexture("_x_shadowMap", shadowMap); 
        //Shader.SetGlobalFloat("_x_dist", shadowDist);
        light.RenderWithShader(Shader.Find("XWorld/ShadowMapGen"), null);

        //Vector4 cubeP = (light.projectionMatrix * light.worldToCameraMatrix).MultiplyPoint(cube.position);
        //cubeP = (cubeP + new Vector4(1, 1, 1, 0)) / 2;
        //Debug.LogError(cubeP.x + "   " + cubeP.y + "   " + (cubeP.z) + "   " + cubeP.w);
	}
}
