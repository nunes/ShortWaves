using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode] // Esto hace que funcione incluso sin dar Play
public class GridOverlay : MonoBehaviour
{
    public bool mostrarRejilla = true;
    public Color colorLinea = Color.white;
    [Range(0, 1)] public float opacidad = 0.5f;

    private Material lineMaterial;

    void OnEnable()
    {
        RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
    }

    void OnDisable()
    {
        RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
    }

    void OnPostRender()
    {
        DrawGrid();
    }

    void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (camera == GetComponent<Camera>())
        {
            DrawGrid();
        }
    }

    void DrawGrid()
    {
        if (!mostrarRejilla) return;
        crearMaterialLinea();

        lineMaterial.SetPass(0);
        GL.PushMatrix();
        GL.LoadOrtho();
        GL.Begin(GL.LINES);
        GL.Color(new Color(colorLinea.r, colorLinea.g, colorLinea.b, opacidad));

        // Líneas Verticales (Regla de tercios)
        dibujarLinea(0.33f, 0, 0.33f, 1);
        dibujarLinea(0.66f, 0, 0.66f, 1);

        // Líneas Horizontales (Regla de tercios)
        dibujarLinea(0, 0.33f, 1, 0.33f);
        dibujarLinea(0, 0.66f, 1, 0.66f);

        // Cruz Central (Opcional - Comenta si no la quieres)
        // dibujarLinea(0.5f, 0.48f, 0.5f, 0.52f); // Pequeña marca vertical centro
        // dibujarLinea(0.48f, 0.5f, 0.52f, 0.5f); // Pequeña marca horizontal centro

        GL.End();
        GL.PopMatrix();
    }

    void dibujarLinea(float x1, float y1, float x2, float y2)
    {
        GL.Vertex3(x1, y1, 0);
        GL.Vertex3(x2, y2, 0);
    }

    void crearMaterialLinea()
    {
        if (!lineMaterial)
        {
            // Unity tiene un shader oculto simple para dibujar líneas
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            lineMaterial.SetInt("_ZWrite", 0);
        }
    }
}