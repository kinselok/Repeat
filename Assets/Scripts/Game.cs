using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{
    Calculating Figure1 = new Calculating();
    Calculating SampleFigure = new Calculating();
    public GameObject trail;
    public float Borders = 0.40f;
    public LineRenderer figure;
    public Text timerText;
    private float timer;
    private float timerCount;
    private bool isStarted;
    private Dictionary<int, List<Vector3>> SamplesData;
    private int SampleIndex;
    public GameObject panel;
    private Color changedColor;
    public Text LabelScore;
    private int score;
    public GameObject AnimPlus1;
    public Text ResultScore;



    void Start()
    {
        timerCount = 5.0f;
        score = 0;
        timer = timerCount;
        isStarted = false;
        timerText.text = timer.ToString("F2");
        SamplesData = new Dictionary<int, List<Vector3>>();
        SampleIndex = 0;
        SamplesData.Add(0, new List<Vector3> { new Vector3(0.0f, 0.0f, 0.0f),
                                               new Vector3(30.0f, 30.0f, 0.0f),
                                               new Vector3(-30.0f, 30.0f, 0.0f),
                                               new Vector3(0.0f, 0.0f, 0.0f)});

        SamplesData.Add(1, new List<Vector3> { new Vector3(-30.0f, 30.0f, 0.0f),
                                               new Vector3(30.0f, 30.0f, 0.0f),
                                               new Vector3(30.0f, -30.0f, 0.0f),
                                               new Vector3(-30.0f, -30.0f, 0.0f),
                                               new Vector3(-30.0f, 30.0f, 0.0f)});

        SamplesData.Add(2, new List<Vector3> { new Vector3(-30.0f, 30.0f, 0.0f),
                                               new Vector3(30.0f, 30.0f, 0.0f),
                                               new Vector3(-30.0f, -30.0f, 0.0f),
                                               new Vector3(-30.0f, 30.0f, 0.0f)});
    }


    void Update()
    {
       if (!isStarted)
        {
            SampleFigure.points.Clear();
            SampleFigure.points.AddRange(SamplesData[SampleIndex]);
            Vector3[] t = new Vector3[SampleFigure.points.Count];
            for (int i = 0; i < SampleFigure.points.Count; i++) t[i] = new Vector3(SampleFigure.points[i].x, SampleFigure.points[i].y, 0.0f);
            figure.positionCount = SampleFigure.points.Count;
            figure.SetPositions(t);
            timerText.text = timer.ToString("F2");
        }
        if (timer > 0 && isStarted)
        {
            timer -= Time.deltaTime;
            timerText.text = timer.ToString("F2");
        }
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 tmp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            trail.transform.position = new Vector3(tmp.x, tmp.y, 0.0f);
            trail.SetActive(true);
            isStarted = true;
            figure.positionCount = 0;
        }
        if (timer <= 0)
        {
            timerText.text = (0.0f).ToString("F2");
            panel.SetActive(true);
            ResultScore.text = score.ToString();
            trail.SetActive(false);
        }
        if (Input.GetMouseButtonUp(0) && timer > 0)
        {
            timerText.text = timer.ToString("F2");
            for (int i = 0; i < Figure1.points.Count - 1; i++)
            {
                if (Figure1.Len(Figure1.points[i], Figure1.points[i + 1]) < 3.0f)
                {
                    Figure1.points.RemoveAt(i + 1);
                    i--;
                }
            }//удаление близких точек
            if (Figure1.points.Count > 2)
            {
                isStarted = false;
                if (Counting() < 3.0f)
                {
                    if (SampleIndex + 1 < SamplesData.Count) SampleIndex++;
                    else SampleIndex = 0;
                    score++;
                    timer = timerCount - 0.25f * score;
                    LabelScore.text = score.ToString();
                    Instantiate(AnimPlus1);
                    trail.SetActive(false);
                    trail.GetComponent<TrailRenderer>().Clear();
                }
                Figure1.points.Clear();
            }

        }

    }

    public void Restart()
    {
        SceneManager.LoadScene("Game");
    }

    float Counting()
    {
        List<int> triangles = new List<int>();
        List<Vector3> buff = new List<Vector3>();
        buff.AddRange(Figure1.points);
        Vector2 HeightWidth1 = Figure1.WidthHeight();
        Vector2 HeightWidth2 = SampleFigure.WidthHeight();
        if (Mathf.Abs(HeightWidth1.x / HeightWidth2.x - HeightWidth1.y / HeightWidth2.y) > 0.3f) return 20.0f;

        for (int i = 0; i < Figure1.points.Count - 1; i++)
        {
            for (int k = 0; k < Figure1.points.Count - 1; k++)
            {
                if (Mathf.Abs(i - k) > 1)
                {
                    Vector2 cross = Figure1.CrossPoint(Figure1.points[i], Figure1.points[i + 1], Figure1.points[k], Figure1.points[k + 1]);
                    if (Figure1.ConfirmPoint(Figure1.points[i], Figure1.points[i + 1], cross) && Figure1.ConfirmPoint(Figure1.points[k], Figure1.points[k + 1], cross))
                    {
                        triangles.Add(k);
                        triangles.Add(i);
                    }
                }
            }
        }
        if (triangles.Count != 0)
        {
            triangles.Sort();
            Figure1.points.Clear();
            Figure1.points.AddRange(buff.GetRange(0, triangles[0]));
            Figure1.points.AddRange(buff.GetRange(triangles[triangles.Count - 1], buff.Count - triangles[triangles.Count - 1]));
            Figure1.Triangulation();

            Figure1.points.Clear();
            Figure1.points.AddRange(buff.GetRange(triangles[0], triangles[triangles.Count - 1] - triangles[0]));
            Figure1.Triangulation();
            Figure1.points.Clear();
            Figure1.points.AddRange(buff);
        }
        else Figure1.Triangulation();
        Vector2 CentreDraw = Figure1.FindCentrePolygon();
        SampleFigure.Triangulation();
        Vector2 CentreSample = SampleFigure.FindCentrePolygon();
       
        //сдвиг и зум 2-й фигуры
        

        float coef = (HeightWidth1.x / HeightWidth2.x + HeightWidth1.y / HeightWidth2.y) / 2;
        CentreSample.x = CentreSample.x * coef;
        CentreSample.y = CentreSample.y * coef;
        Vector2 shift = new Vector2(CentreDraw.x - CentreSample.x, CentreDraw.y - CentreSample.y);

        CentreSample.x = CentreSample.x + shift.x;
        CentreSample.y = CentreSample.y + shift.y;
        for (int i = 0; i < SampleFigure.points.Count; i++)
        {
            var temp = SampleFigure.points[i];
            temp.x = temp.x * coef;
            temp.y = temp.y * coef;
            temp.x = temp.x + shift.x;
            temp.y = temp.y + shift.y;
            SampleFigure.points[i] = temp;
        }


        float counterLosses = 0f;
        foreach (var a in Figure1.points)
        {
            counterLosses += CalcMiss(a, CentreSample);
        }

        return counterLosses / (float)Figure1.points.Count;
    }

    void OnMouseDrag()
    {
        Vector2 tmp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Figure1.points.Add(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        trail.transform.position = new Vector3(tmp.x, tmp.y, 0.0f);
    }
    float CalcMiss(Vector2 a, Vector2 Centre)
    {
        List<float> Misses = new List<float>();
        for (int i = 0; i < SampleFigure.points.Count - 1; i++)
        {
            Vector2 cross = SampleFigure.CrossPoint(SampleFigure.points[i], SampleFigure.points[i + 1], Centre, a);
            if (SampleFigure.ConfirmPoint(SampleFigure.points[i], SampleFigure.points[i + 1], cross) && SampleFigure.ConfirmPoint(Centre, a, cross)) Misses.Add(SampleFigure.Len(a, cross));
        }
        Misses.Sort();
        if (Misses.Count != 0) return Misses[0];
        else return 0.0f;
    }
}

public class Calculating
{
    public List<Vector3> triangles;
    public List<Vector3> points;
    public Calculating()
    {
        points = new List<Vector3>();
        triangles = new List<Vector3>();
    }
    private void Direction()
    {

        int counterM = 0;
        int counterL = 0;
        for (int i = 0; i < points.Count - 2; i++)
        {
            if (TurnForDirection(points[i], points[i + 1], points[i + 2])) counterM++;
            else counterL++;
        }

        float Direction = 0.0f;
        if (counterL < counterM) Direction = 1.0f; //right
        else Direction = 0.0f; //left
        for (int i = 0; i < points.Count; i++)
        {

            points[i] = new Vector3(points[i].x, points[i].y, Direction);

        }
    }//направление рисования
    public bool TurnForDirection(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        Vector2 vec1 = new Vector2(p3.x - p1.x, p3.y - p1.y);
        Vector2 vec2 = new Vector2(p2.x - p1.x, p2.y - p1.y);
        float vec3 = vec1.x * vec2.y - vec1.y * vec2.x;

        if (vec3 > 0.0f) return true;
        else return false;
    }
    public Vector2 WidthHeight()
    {
        Vector2 max = new Vector2(points[0].x, points[0].y);
        Vector2 min = new Vector2(points[0].x, points[0].y);
        foreach (var a in points)
        {
            if (max.x < a.x) max.x = a.x;
            if (max.y < a.y) max.y = a.y;
            if (min.x > a.x) min.x = a.x;
            if (min.y > a.y) min.y = a.y;
        }

        return new Vector2(max.x - min.x, max.y - min.y);
    }
    public Vector2 CrossPoint(Vector2 a, Vector2 b, Vector2 c, Vector2 d) //точки a и b концы первого отрезка  c и d второго
    {
        Vector2 T = new Vector2(0f, 0f);
        T.x = -((a.x * b.y - b.x * a.y) * (d.x - c.x) - (c.x * d.y - d.x * c.y) * (b.x - a.x)) / ((a.y - b.y) * (d.x - c.x) - (c.y - d.y) * (b.x - a.x));
        T.y = ((c.y - d.y) * (-T.x) - (c.x * d.y - d.x * c.y)) / (d.x - c.x);
        float k = (a.y - b.y) * (d.x - c.x) - (c.y - d.y) * (b.x - a.x);
        if (k != 0) return T;
        else return new Vector2(9999f, 9999f);
    }

    float VectorMultiplication(Vector2 v1, Vector2 v2)
    {
        return v1.x * v2.y - v1.y * v2.x;
    }
    public bool InOrOut(Vector2 Line1, Vector2 Line2, Vector2 point)
    {
        float a = Line2.y - Line1.y;
        float b = Line1.x - Line2.x;
        float c = Line2.x * Line1.y - Line1.x * Line2.y;
        float result = point.x * a + point.y * b + c;

        if (result > 0 && points[0].z == 0.0f) return true;
        else if (result < 0 && points[0].z == 1.0f) return true;
        else return false;
    }
    public bool ConfirmPoint(Vector2 Line1, Vector2 Line2, Vector2 point)
    {
        float a = Line2.y - Line1.y;
        float b = Line1.x - Line2.x;
        float c = Line2.x * Line1.y - Line1.x * Line2.y;
        float result = point.x * a + point.y * b + c;

        if (result <= 0.001f && result >= -0.0001f && Len(Line1, Line2) >= Len(point, Line1) && Len(Line1, Line2) >= Len(point, Line2)) return true;
        else return false;
    }

    public bool ConfirmTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        Vector2 vec1 = new Vector2(p3.x - p1.x, p3.y - p1.y);
        Vector2 vec2 = new Vector2(p2.x - p1.x, p2.y - p1.y);
        float vec3 = vec1.x * vec2.y - vec1.y * vec2.x;

        if (p1.z == 1.0f) if (vec3 > 0.0f) return true;
            else return false;
        else if (p1.z == 0.0f) if (vec3 < 0.0f) return true;
            else return false;
        else return false;
    }
    bool CheckEntry(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        Vector3 point = new Vector3(1000f, 1000f, 0f);

        foreach (var a in points)
        {
            if (a != p1 && a != p2 && a != p3)
            {
                int counter = 0;
                Vector2 WithS1 = CrossPoint(p1, p2, point, a);
                Vector2 WithS2 = CrossPoint(p1, p3, point, a);
                Vector2 WithS3 = CrossPoint(p2, p3, point, a);
                if (ConfirmPoint(p1, p2, WithS1) && ConfirmPoint(point, a, WithS1)) counter++;
                if (ConfirmPoint(p1, p3, WithS2) && ConfirmPoint(point, a, WithS2)) counter++;
                if (ConfirmPoint(p2, p3, WithS3) && ConfirmPoint(point, a, WithS3)) counter++;
                if (counter == 1) return true;
            }
        }
        return false;
    }
    public void Triangulation()
    {
        List<Vector3> buffer = new List<Vector3>();
        buffer.AddRange(points);
        Direction();
        int i = 0;
        while (points.Count != 3)
        {
            if (i > 1000)
            {
                /*foreach (var a in points)
                {
                    Debug.Log(a);
                }*/
                return;
            }
            i++;
            if (ConfirmTriangle(points[0], points[1], points[2]) && !CheckEntry(points[0], points[1], points[2]) && (points[0].z == points[1].z || points[1].z == points[2].z))
            {

                triangles.Add(points[0]);
                triangles.Add(points[1]);
                triangles.Add(points[2]);
                points.RemoveAt(1);
            }
            else
            {
                points.Add(points[0]);
                points.RemoveAt(0);
            }
        }
        if (points.Count == 3) triangles.AddRange(points);
        points.Clear();
        points.AddRange(buffer);
    }

    public float Len(Vector3 p1, Vector3 p2)
    {
        float x = p1.x - p2.x;
        float y = p1.y - p2.y;
        return Mathf.Sqrt(Mathf.Pow(x, 2) + Mathf.Pow(y, 2));
    }

    float AreaTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float a = Len(p1, p2);
        float b = Len(p2, p3);
        float c = Len(p3, p1);
        float p = (a + b + c) / 2;

        return Mathf.Sqrt(p * (p - a) * (p - b) * (p - c));
    }

    Vector2 CentreTriangle(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return new Vector2((p1.x + p2.x + p3.x) / 3, (p1.y + p2.y + p3.y) / 3);
    }

    public Vector2 FindCentrePolygon()
    {
        Vector2 SumOfMultiplication = new Vector2(0.0f, 0.0f);
        float SumOfArea = 0.0f;

        for (int i = 0; i < triangles.Count - 2; i += 3)
        {
            Vector2 TempCentre = new Vector2((triangles[i].x + triangles[i + 1].x + triangles[i + 2].x) / 3, (triangles[i].y + triangles[i + 1].y + triangles[i + 2].y) / 3);
            SumOfMultiplication.x += TempCentre.x * AreaTriangle(triangles[i], triangles[i + 1], triangles[i + 2]);
            SumOfMultiplication.y += TempCentre.y * AreaTriangle(triangles[i], triangles[i + 1], triangles[i + 2]);
            SumOfArea += AreaTriangle(triangles[i], triangles[i + 1], triangles[i + 2]);
        }
        triangles.Clear();
        return new Vector2(SumOfMultiplication.x / SumOfArea, SumOfMultiplication.y / SumOfArea);
    }
}
