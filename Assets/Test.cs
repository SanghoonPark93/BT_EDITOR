using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

public class Ball 
{
    public int idx;
    public double x;
    public double y;

    //정확한 거리가 아닌 비교용 프로퍼티
    public double StartDistance => Math.Pow(x, 2) + Math.Pow(y, 2);

    public bool isBlock;

    public double CurDistance(double x, double y) 
    {
        return Math.Pow((this.x - x), 2) + Math.Pow((this.y - y), 2);
    }

    public bool IsHit(double x, double y) 
    {
        // 반지름 1인 두 원의 교차 여부
        return CurDistance(x, y) < 4;
    }

    public Ball(int idx, double x, double y) 
    {
        this.idx = idx;
        this.x = x;
        this.y = y;
    }
}

public class Solution
{
    public const double BallDiameter = 2;
    public int solution(double[,] objectBallPosList, double[] hitVector)
    {        
        var list = new List<Ball>();
        var length = objectBallPosList.GetLength(0);
        for (var i = 0; i < length; i++) 
        {
            list.Add(new Ball(i, objectBallPosList[i, 0], objectBallPosList[i, 1]));
        }
                
        var xVec = hitVector[0];
        var yVec = hitVector[1];
        var x = 0.0d;
        var y = 0.0d;
        while (list.Any()) 
        {
            x += xVec;
            y += yVec;

            var curDis = Math.Pow(x, 2) + Math.Pow(y, 2);

            foreach (var m in list) 
            {
                if (m.IsHit(x, y))
                    return m.idx;

                if (m.StartDistance < curDis)
                    m.isBlock = true;
            }

            list.RemoveAll(m => m.isBlock);
        }

        return -1;
    }
}
