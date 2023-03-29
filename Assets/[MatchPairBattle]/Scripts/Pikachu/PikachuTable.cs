using System.Collections.Generic;
using UnityEngine;

namespace Pikachu
{
    public class PikachuTable : MonoBehaviour
    {
        [Header("Matrix Properties")]        
        [SerializeField] AnimalButton[] m_buttonTypeList;
        [SerializeField] Vector2 m_tableSize;
        private AnimalButton[,] m_table;

        public AnimalButton[,] Table => m_table;
        public Vector2 TableSize => m_tableSize;
        public int ButtonAmount => (int)((m_tableSize.x - 2) * (m_tableSize.y - 2));

        [Header("Additional Properties")]                      
        [SerializeField] AudioSource m_clickAudio;

        public int PairAmount => m_pairAmount;
        public bool IsTableEmpty => (m_pairAmount <= 0) ? true : false;        
        
        private int m_pairAmount;
        private const float m_buttonSize = 75f;
        private AnimalButton m_startObject;
        private AnimalButton m_endObject;        
        private List<AnimalButton> m_buttonList = new List<AnimalButton>();

        //Draw Line Function
        [SerializeField]
        private  GameObject linePrefab;
        private struct Point
        {
            public int x;
            public int y;
            public Point(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        };

        #region Table Creator        
        [ContextMenu("Create table")]
        public void CreatePlayerTable()
        {
            int row = (int) m_tableSize.x;
            int column = (int)m_tableSize.y;

            m_table = new AnimalButton[row, column];
            m_pairAmount = ((row - 2) * (column - 2)) / 2;

            // Create normal button
            for (int i = 0; i < m_pairAmount; i++)
            {
                // Create random button type
                int random = Random.Range(1, m_buttonTypeList.Length);

                // Get random button location in table
                int x1 = 1, y1 = 1;
                while (m_table[x1, y1] != null)
                {
                    x1 = Random.Range(1, row - 1);
                    y1 = Random.Range(1, column - 1);
                }
                m_table[x1, y1] = Instantiate(m_buttonTypeList[random], this.transform);
                m_table[x1, y1].transform.SetParent(this.transform, false);

                int x2 = 1, y2 = 1;
                while (m_table[x2, y2] != null)
                {
                    x2 = Random.Range(1, row - 1);
                    y2 = Random.Range(1, column - 1);
                }
                m_table[x2, y2] = Instantiate(m_buttonTypeList[random], this.transform);
                m_table[x2, y2].transform.SetParent(this.transform, false);
            }

            // Create emtpy button
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < column; j++)
                {
                    if ((i == 0) || (i == row - 1) || (j == 0) || (j == column - 1))
                    {
                        m_table[i, j] = Instantiate(m_buttonTypeList[0], this.transform);
                        m_table[i, j].transform.SetParent(transform, false);                        
                    }
                }
            }

            // Set position
            float rowStart = - ((m_tableSize.x - 1) / 2) * m_buttonSize; float columnStart = - ((m_tableSize.y - 1) / 2) * m_buttonSize;
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < column; j++)
                {                   
                    // Set position and location of button
                    m_table[i, j].m_RectTransform.localPosition = new Vector3(rowStart + i * m_buttonSize, columnStart + j * m_buttonSize,  0);
                    m_table[i, j].x = i; m_table[i, j].y = j;
                    m_buttonList.Add(m_table[i, j]);
                }
            }
            GameManagement.Instance.SendPlayerTableData();
        }                
        public void CreateTable(byte[,] buttonCode, Vector2 tableSize)
        {
            // Set position & table size
            m_tableSize = tableSize;
            float rowStart = -((tableSize.x - 1) / 2) * m_buttonSize; float columnStart = -((tableSize.y - 1) / 2) * m_buttonSize;
            // Create table
            m_table = new AnimalButton[(int)tableSize.x, (int)tableSize.y];
            m_pairAmount = (int)((tableSize.x - 2) * (tableSize.y - 2) / 2);

            for (int i = 0; i < tableSize.x; i++)
            {
                for (int j = 0; j < tableSize.y; j++)
                {
                    // Create button
                    var button = Instantiate(m_buttonTypeList[buttonCode[i, j]], this.transform);
                    button.GetComponent<AnimalButton>().m_RectTransform.localPosition = new Vector3(rowStart + i * m_buttonSize, columnStart + j * m_buttonSize, 0);
                    m_table[i, j] = button;
                    m_table[i, j].x = i;
                    m_table[i, j].y = j;
                }
            }
        }
        #endregion

        #region Table Execute Progress
        public void OnButtonClicked(AnimalButton choosenObject)
        {
            m_clickAudio.PlayOneShot(m_clickAudio.clip);
            if (m_startObject == null)
            {
                m_startObject = choosenObject;
                choosenObject.transform.GetChild(0).gameObject.SetActive(true);
                return;
            }
            if (m_startObject == choosenObject)
            {
                m_startObject = null;
                choosenObject.transform.GetChild(0).gameObject.SetActive(false);
                return;
            }
            else
            {
                m_endObject = choosenObject;
                choosenObject.transform.GetChild(0).gameObject.SetActive(true);
            }

            if (CheckValidPair(m_startObject, m_endObject))
            {                
                HidePair(m_startObject, m_endObject);

                OnMatchedPair(m_startObject, m_endObject);
                GameManagement.Instance.SendPlayerPairData(m_startObject, m_endObject, m_endObject.Type);    
            }

            // Turn off choosen mode
            m_startObject.transform.GetChild(0).gameObject.SetActive(false);
            m_endObject.transform.GetChild(0).gameObject.SetActive(false);
            
            // Renew object
            m_startObject = null;
            m_endObject = null;
        }
        public void HidePair(AnimalButton start, AnimalButton end)
        {
            m_pairAmount--;
            start.OnHideButton();
            end.OnHideButton();            
        }
        private bool CheckValidPair(AnimalButton button1, AnimalButton button2)
        {
            if (button1.Type != button2.Type) return false;
            button1.m_IsObstacle = false; button2.m_IsObstacle = false;

            // if two pair are on a same column or same row
            if (button1.x == button2.x)
            {
                if (CheckOnRowX(button1.y, button2.y, button1.x))
                {

                    return true;
                }
            }
            if (button1.y == button2.y)
            {
                if (CheckOnColumnY(button1.x, button2.x, button1.y))
                {

                    return true;
                }
            }

            // Check with Rectangle
            if (CheckOnRectHorizontal(new Point(button1.x, button1.y), new Point(button2.x, button2.y)))
            {
                return true;
            }
            if (CheckOnRectVertical(new Point(button1.x, button1.y), new Point(button2.x, button2.y)))
            {
                return true;
            }
            // Expandation Check
            // Horizontal
            if (CheckOnHorizontalExpand(new Point(button1.x, button1.y), new Point(button2.x, button2.y), 1))
            {
                return true;
            }
            if (CheckOnHorizontalExpand(new Point(button1.x, button1.y), new Point(button2.x, button2.y), -1))
            {
                return true;
            }
            // Vertical
            if (CheckOnVerticalExpand(new Point(button1.x, button1.y), new Point(button2.x, button2.y), 1))
            {
                return true;
            }

            if (CheckOnVerticalExpand(new Point(button1.x, button1.y), new Point(button2.x, button2.y), -1))
            {
                return true;
            }

            button1.m_IsObstacle = true; button2.m_IsObstacle = true;
            return false;
        }
        #endregion

        #region Pikachu Algorithm

        // Check pair on a same line or same column
        private bool CheckOnRowX(int y1, int y2, int x)
        {
            int start = Mathf.Min(y1, y2);
            int end = Mathf.Max(y1, y2);

            for (int y = start; y <= end; y++)
            {
                Debug.Log("RowX check: "+x + "-"+y);
                // Has object between y1 and y2 on the line x
                if (m_table[x, y].m_IsObstacle)
                {                  
                    return false;
                }
            }
            return true;            
        }
        private bool CheckOnColumnY(int x1, int x2, int y)
        {
            int start = Mathf.Min(x1, x2);
            int end = Mathf.Max(x1, x2);

            for (int x = start; x <= end; x++)
            {
                
                // Has object between y1 and y2 on the line x
                if (m_table[x, y].m_IsObstacle)
                {
                    
                    return false;
                }
            }

            return true;            
        }

        // Check pair in a rectangle
        private bool CheckOnRectHorizontal(Point point1, Point point2)
        {
            /* Start from upper-point to lower-point
           start ______<1>______
                                |
                               <2>
                                |
                                _______<3>_______ end         
        */
            
            Point startPoint = point1; Point endPoint = point2;
            if (point1.y > point2.y) // The bigger of  y-index, the lower of point's position
            {
                startPoint = point2;
                endPoint = point1;
            }

            for (int y = startPoint.y; y <= endPoint.y; y++)
            {
                // if line <1>, <2> and <3> are exist then this Rect is  exist too
                    // if there is no line on row x then return false;
                if (!CheckOnRowX(startPoint.y, y, startPoint.x)) return false;
                // when line <1> is exist then we check line <2> and <3> to define if there is a valid path for start to end
                else 
                {

                    if (CheckOnColumnY(startPoint.x, endPoint.x, y) && CheckOnRowX(y, endPoint.y, endPoint.x))
                        return true;
                }                
            }            
            return false;
        }
        private bool CheckOnRectVertical(Point point1, Point point2)
        {
            /* Start from left-point to right-point
                  start 
                    |
                   <1>
                    |
                    _______<2>_______ 
                                     |
                                    <3>
                                     |
                                    end
               */

            Point startPoint = point1; Point endPoint = point2;
            if (point1.x > point2.x)
            {
                startPoint = point2;
                endPoint = point1;
            }
            for (int x = startPoint.x; x <= endPoint.x; x++)
            {
                // if line <1>, <2> and <3> are exist then this Rect is  exist too

                    // if there is no line on row x then return false;
                if (!CheckOnColumnY(startPoint.x, x, startPoint.y)) return false;
                
                // when line <1> is exist then we check line <2> and <3> to define if there is a valid path for start to end
                else 
                {

                    if (CheckOnRowX(startPoint.y, endPoint.y, x) && CheckOnColumnY(x, endPoint.x, endPoint.y))
                    return true;
                }
            }
            return false;
        }

        // Expandation Check
        private bool CheckOnHorizontalExpand(Point point1, Point point2, int direction)
        {
            /*
             start_______________<1>_______________
                                                   |
                                                  <2>
                                    end____<3>_____|


            */
            Point startPoint = point1; Point endPoint = point2;
            if (point1.y > point2.y)
            {
                startPoint = point2;
                endPoint = point1;
            }

            int column = endPoint.y; int row = startPoint.x;
            if (direction < 0)
            {
                column = startPoint.y;
                row = endPoint.x;
            }
            
            if (CheckOnRowX(startPoint.y, endPoint.y, row)) // if line <1> exist
            {
                
                while (!m_table[startPoint.x, column].m_IsObstacle && !m_table[endPoint.x, column].m_IsObstacle)
                {
                    
                    if (CheckOnColumnY(startPoint.x, endPoint.x, column))
                    {

                        return true;
                    }

                    column += direction;
                    if (column != Mathf.Clamp(column, 0, m_tableSize.y)) break; 
                }
            }
            return false;
        }
        private bool CheckOnVerticalExpand(Point point1, Point point2, int direction)
        {
            Point startPoint = point1; Point endPoint = point2;

            if (point1.x > point2.x)
            {
                startPoint = point2;
                endPoint = point1;
            }

            int row = endPoint.x; int column = startPoint.y;
            if (direction < 0)
            {
                row = startPoint.x;
                column = endPoint.y;
            }

            if (CheckOnColumnY(startPoint.x, endPoint.x, column))
            {
                while (!m_table[row, startPoint.y].m_IsObstacle && !m_table[row, endPoint.y].m_IsObstacle)
                {
                    if (CheckOnRowX(startPoint.y, endPoint.y, row))
                    {

                        return true;
                    }

                    row += direction;
                    if (row != Mathf.Clamp(row, 0, m_tableSize.x))
                    {
                        break;
                    } 
                }
            }
            return false;
        }

        #endregion

        #region DrawLine

        public void OnMatchedPair(AnimalButton button1, AnimalButton button2)
        {

            // Draw a line between the two matching pairs
            Vector3 start, end, corner, corner2;
            start = new Vector3(button1.transform.localPosition.x, button1.transform.localPosition.y, 0);
            end = new Vector3(button2.transform.localPosition.x, button2.transform.localPosition.y, 0);

            //Draw U shape Right-Left-Outside
            if (button1.x == button2.x && button1.x == 1 || button1.x == button2.x && button1.x == 10)
            {
                switch (button1.x)
                {
                    case 1:
                        corner = start - new Vector3(m_buttonSize, 0, 0);
                        DrawLine(start, corner);
                        DrawLine_L_Doc(corner, end, button1, button2);

                        break;
                    case 10:
                        corner = start + new Vector3(m_buttonSize, 0, 0);
                        DrawLine(start, corner);
                        DrawLine_L_Doc(corner, end, button1, button2);
                        break;
                }
            }
            //Draw U shape Up-Down_Outside
            if (button1.y == button2.y && button1.y == 1 || button1.y == button2.y && button1.y == 10)
            {
                switch (button1.y)
                {
                    case 1:
                        corner = start - new Vector3(0, m_buttonSize, 0);
                        DrawLine(start, corner);
                        DrawLine_L_Ngang(corner, end, button1, button2);

                        break;
                    case 10:
                        corner = start + new Vector3(0, m_buttonSize, 0);
                        DrawLine(start, corner);
                        DrawLine_L_Ngang(corner, end, button1, button2);
                        break;
                }
            }

            //Draw Straight Line
            if (button1.x == button2.x && button1.x != 1 && button2.x != 10)
            {
                Debug.Log("3");
                if (CheckOnRowX(button1.y, button2.y, button1.x))
                {
                    DrawLine(start, end);
                    return;
                }
           
            }
            if (button1.y == button2.y && button1.y != 1 && button2.y != 10)
            {
                Debug.Log("4");
                if (CheckOnColumnY(button1.x, button2.x, button1.y))
                {
                    DrawLine(start, end);
                }
                  
            }

            //Draw L line
                if (!CheckOnColumnY(button1.x, button2.x, button1.y))
                {
                    var count = 0;
                    //Check tu vi tri y di len tren
                        Debug.Log("y1<y2");
                        for (int y = button1.y + 1; y <= 11; y++)
                        {
                            count++;
                            if (CheckOnColumnY(button1.x, button2.x, y))
                            {
                                corner = start + new Vector3(0, m_buttonSize * count, 0);
                                Debug.Log("Corner1: " + corner);
                                if (CheckOnRowX(button2.y,Get_Y_On_Vector(corner),button2.x))
                                {
                                    corner2 = end - new Vector3(0, m_buttonSize * (button2.y - Get_Y_On_Vector(corner)), 0);
                                    if (CheckOnRowX(button1.y, Get_Y_On_Vector(corner), button1.x))
                                    {
                                      Debug.Log("Corner2: " + corner2);
                                      DrawLine(start, corner);
                                      DrawLine(corner, corner2);
                                      DrawLine(corner2, end);
                                      break;
                                    }
                            }    
                                        
                            }
                        }
                        count = 0;
                        for (int y = button1.y - 1; y >= 0; y--)
                        {
                            count--;
                            if (CheckOnColumnY(button1.x, button2.x, y))
                            {
                                corner = start + new Vector3(0, m_buttonSize * count, 0);
                                Debug.Log("Corner1: " + corner);
                                if (CheckOnRowX(button2.y, Get_Y_On_Vector(corner), button2.x))
                                {
                                    corner2 = end - new Vector3(0, m_buttonSize * (button2.y - Get_Y_On_Vector(corner)), 0);
                                    if (CheckOnRowX(button1.y, Get_Y_On_Vector(corner), button1.x))
                                    {
                                    Debug.Log("Corner2: " + corner2);
                                    DrawLine(start, corner);
                                    DrawLine(corner, corner2);
                                    DrawLine(corner2, end);
                                    break;
                                    }
                                }
                            }
                        }
                }
                if (!CheckOnRowX(button1.y,button2.y,button1.x))
                {
                    var count = 0;
                    for(int x=button1.x+1;x<=11;x++)
                        {
                            count++;
                            if(CheckOnRowX(button1.y,button2.y,x))
                            {
                                corner = start + new Vector3(m_buttonSize* count, 0, 0);
                                Debug.Log("Corner1: " + corner);
                                if (CheckOnColumnY(button2.x,Get_X_On_Vector(corner),button2.y))
                                {
                                    corner2 = end - new Vector3(m_buttonSize * (button2.x - Get_X_On_Vector(corner)), 0, 0);
                                    if (CheckOnColumnY(button1.x, Get_X_On_Vector(corner), button1.y))
                                    {
                                    Debug.Log("Corner2: " + corner2);
                                    DrawLine(start, corner);
                                    DrawLine(corner, corner2);
                                    DrawLine(corner2, end);
                                    break;
                                    }
                                }
                            }    
                                
                        }
                    count = 0;
                    for(int x=button1.x-1;x>=0;x--)
                        {
                            count--;
                            if (!CheckOnRowX(button1.y, button2.y, x))
                            {
                                corner = start + new Vector3(m_buttonSize * count, 0, 0);
                                Debug.Log("Corner1: " + corner);
                                if (CheckOnColumnY(button2.x, Get_X_On_Vector(corner), button2.y))
                                {
                                    corner2 = end - new Vector3(m_buttonSize * (button2.x - Get_X_On_Vector(corner)), 0, 0);
                                    if (CheckOnColumnY(button1.x, Get_X_On_Vector(corner), button1.y))
                                    {
                                    Debug.Log("Corner2: " + corner2);
                                    DrawLine(start, corner);
                                    DrawLine(corner, corner2);
                                    DrawLine(corner2, end);
                                    break;
                                    }
                                }
                            }
                        }    
                       
                }    
                if(CheckOnColumnY(button1.x,button2.x,button1.y))
                {
                    if(CheckOnRowX(button2.y,button1.y,button2.x))
                    {
                        Debug.Log("L_Ngang");
                        DrawLine_L_Ngang(start, end, button1, button2);
                    }

                }
                if(CheckOnRowX(button1.y, button2.y,button1.x))
                {
                    if(CheckOnColumnY(button2.x,button1.x,button2.y))
                    {
                        Debug.Log("L_DOc");
                        DrawLine_L_Doc(start, end, button1, button2);
                    }
                }
                
        }
        // Draw a line between the two given points

        public void DrawLine_L_Ngang(Vector3 start, Vector3 end, AnimalButton button1, AnimalButton button2)
        {

            Vector3 corner = start + new Vector3(end.x - start.x, 0, 0);

            if (CheckOnColumnY_Corner(button1.x, button2.x, button1.y, Get_Y_On_Vector(corner)))
            {
                Debug.Log("Corner.Cordi: (" + button1.x + "-" + Get_Y_On_Vector(corner) + ")");
                DrawLine(start, corner);
                DrawLine(corner, end);
            }


        }

        public void DrawLine_L_Doc(Vector3 start, Vector3 end, AnimalButton button1, AnimalButton button2)
        {
            Vector3 corner = start + new Vector3(0, end.y - start.y, 0);

            Debug.Log(corner.x + "-" + corner.y);


            if (CheckOnRowX_Corner(button1.y, button2.y, button1.x, Get_X_On_Vector(corner)))
            {
                Debug.Log("Corner.Cordi: (" + button1.x + "-" + Get_Y_On_Vector(corner) + ")");
                DrawLine(start, corner);
                DrawLine(corner, end);
            }

        }
        private bool CheckOnColumnY_Corner(int x1, int x2, int y1, int corner)
        {

            int temp = y1;
            y1 = corner;
            corner = temp;

            int start = Mathf.Min(x1, x2);
            int end = Mathf.Max(x1, x2);

            for (int x = start; x <= end; x++)
            {
                // Has object between y1 and y2 on the line x
                if (m_table[x, y1].m_IsObstacle)
                {
                    return false;
                }
            }
            return true;

        }
        private bool CheckOnRowX_Corner(int y1, int y2, int x, int corner)
        {

            int temp = x;
            x = corner;
            corner = temp;

            int start = Mathf.Min(y1, y2);
            int end = Mathf.Max(y1, y2);
            for (int y = start; y <= end; y++)
            {
                // Has object between y1 and y2 on the line x
                if (m_table[x, y].m_IsObstacle)
                {
                    return false;
                }
            }
            return true;
        }

        public void DrawLine(Vector3 start, Vector3 end)
        {
            // Calculate the midpoint of the line
            Vector3 midPoint = (start + end) / 2f;

            // Create a new line object using the prefab
            GameObject line = Instantiate(linePrefab, midPoint, Quaternion.identity, transform);

            // Rotate the line to face the end point
            line.transform.right = end - start;

            // Scale the line to match the distance between the two points
            Vector3 scale = line.transform.localScale;
            scale.x = Vector3.Distance(start, end);
            scale.y = 10;
            line.transform.localScale = scale;

            // Set the Z position of the line
            line.transform.localPosition = new Vector3(midPoint.x, midPoint.y, 0);

            Destroy(line, 0.2f);

        }

        //Get Value X on Vector3 corner button
        int Get_X_On_Vector(Vector3 corner)
        {

            // Loop over the children and check each one for a matching button
            foreach (AnimalButton child in m_buttonList)
            {
                AnimalButton button = child.GetComponent<AnimalButton>();
                if (button != null && Mathf.Approximately(button.transform.localPosition.x, corner.x) && Mathf.Approximately(button.transform.localPosition.y, corner.y))
                {
                    return button.x;
                }
            }

            // No matching button found
            return -1;
        }
        //Get Value Y on Vector3 corner button
        int Get_Y_On_Vector(Vector3 corner)
        {

            // Loop over the children and check each one for a matching button
            foreach (AnimalButton child in m_buttonList)
            {
                AnimalButton button = child.GetComponent<AnimalButton>();
                if (button != null && Mathf.Approximately(button.transform.localPosition.x, corner.x) && Mathf.Approximately(button.transform.localPosition.y, corner.y))
                {
                    return button.y;
                }
            }

            // No matching button found
            return -1;
        }
        #endregion
    }
}