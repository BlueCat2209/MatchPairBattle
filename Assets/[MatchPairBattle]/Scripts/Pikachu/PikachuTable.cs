using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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
        [SerializeField] GameObject m_linePrefab;
        [SerializeField] AudioSource m_clickAudio;

        public int PairAmount => m_pairAmount;
        public bool IsTableEmpty => (m_pairAmount <= 0) ? true : false;        
        
        private int m_pairAmount;
        private const float m_buttonSize = 75f;
        private AnimalButton m_startObject;
        private AnimalButton m_endObject;        
        private List<AnimalButton> m_buttonList = new List<AnimalButton>();
        
        private struct Point
        {
            public int x;
            public int y;
            public override string ToString()
            {
                return "{" + x + ";" + y + "}";
            }
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
                if (CheckOnColumnX(button1.y, button2.y, button1.x))
                {
                    DrawLine(button1.transform.localPosition, button2.transform.localPosition);
                    return true;
                }
            }
            if (button1.y == button2.y)
            {
                if (CheckOnRowY(button1.x, button2.x, button1.y))
                {
                    DrawLine(button1.transform.localPosition, button2.transform.localPosition);
                    return true;
                }
            }

            Vector3 middle1; Vector3 middle2;
            // Check with Rectangle
            if (CheckOnRectHorizontal(new Point(button1.x, button1.y), new Point(button2.x, button2.y), out middle1, out middle2))
            {
                DrawLine_U(button1.transform.localPosition, middle1, middle2, button2.transform.localPosition);
                return true;
            }

            if (CheckOnRectVertical(new Point(button1.x, button1.y), new Point(button2.x, button2.y), out middle1, out middle2))
            {
                DrawLine_U(button1.transform.localPosition, middle1, middle2, button2.transform.localPosition);
                return true;
            }

            // Expandation Check
            // Horizontal
            if (CheckOnHorizontalExpand(new Point(button1.x, button1.y), new Point(button2.x, button2.y), 1, out middle1, out middle2))
            {
                DrawLine_U(button1.transform.localPosition, middle1, middle2, button2.transform.localPosition);
                return true;
            }
            if (CheckOnHorizontalExpand(new Point(button1.x, button1.y), new Point(button2.x, button2.y), -1, out middle1, out middle2))
            {
                DrawLine_U(button1.transform.localPosition, middle1, middle2, button2.transform.localPosition);
                return true;
            }
            // Vertical
            if (CheckOnVerticalExpand(new Point(button1.x, button1.y), new Point(button2.x, button2.y), 1, out middle1, out middle2))
            {
                DrawLine_U(button1.transform.localPosition, middle1, middle2, button2.transform.localPosition);
                return true;
            }
            if (CheckOnVerticalExpand(new Point(button1.x, button1.y), new Point(button2.x, button2.y), -1, out middle1, out middle2))
            {
                DrawLine_U(button1.transform.localPosition, middle1, middle2, button2.transform.localPosition);
                return true;
            }

            button1.m_IsObstacle = true; button2.m_IsObstacle = true;
            return false;
        }
        #endregion

        #region Draw Line
        private void DrawLine(Vector3 start, Vector3 end)
        {
            var middlePoint = (start + end) / 2;
            var lineLength = (end - start).magnitude;
            var line = Instantiate(m_linePrefab, transform);
            
            line.transform.localScale = new Vector3(lineLength, 1, 1);
            line.transform.right = (middlePoint - start).normalized;
            line.transform.localPosition = middlePoint;

            Destroy(line, 0.5f);
        }
        private void DrawLine_L(Vector3 start, Vector3 middle, Vector3 end)
        {
            DrawLine(start, middle);
            DrawLine(middle, end);
        }
        private void DrawLine_U(Vector3 start, Vector3 middle1, Vector3 middle2, Vector3 end)
        {
            DrawLine(start, middle1);
            DrawLine_L(middle1, middle2, end);
        }
        #endregion

        #region Pikachu Algorithm

        // Check pair on a same line or same column
        private bool CheckOnColumnX(int y1, int y2, int x)
        {
            int start = Mathf.Min(y1, y2);
            int end = Mathf.Max(y1, y2);

            for (int y = start; y <= end; y++)
            {
                // Has object between y1 and y2 on the line x
                if (m_table[x, y].m_IsObstacle)
                {
                    Debug.Log("[Pikachu Algorithm][CheckOnColumnX] " + y1 + " ->" + y2 + " on column " + x + " is BLOCK!");
                    return false;
                }
            }
            Debug.Log("[Pikachu Algorithm][CheckOnColumnX] " + y1 + " ->" + y2 + " on column " + x + " is CLEAR!");
            return true;            
        }
        private bool CheckOnRowY(int x1, int x2, int y)
        {
            int start = Mathf.Min(x1, x2);
            int end = Mathf.Max(x1, x2);

            for (int x = start; x <= end; x++)
            {
                // Has object between y1 and y2 on the line x
                if (m_table[x, y].m_IsObstacle)
                {
                    Debug.Log("[Pikachu Algorithm][CheckOnRowY] " + x1 + " ->" + x2 + " on row " + y + " is BLOCK!");
                    return false;
                }
            }
            Debug.Log("[Pikachu Algorithm][CheckOnRowY] " + x1 + " ->" + x2 + " on row " + y + " is CLEAR!");
            return true;            
        }

        // Check pair in a rectangle
        private bool CheckOnRectHorizontal(Point point1, Point point2, out Vector3 middle1, out Vector3 middle2)
        {
            /* Start from upper-point to lower-point
               start ______<1>______
                                    |
                                   <2>
                                    |
                                    _______<3>_______ end         
            */
            Point startPoint = point1; Point endPoint = point2;
            middle1 = Vector3.zero; middle2 = Vector3.zero;

            if (point1.x > point2.x) // Must be started from the lower column index on Horizontal (because of the FOR loop)
            {
                startPoint = point2;
                endPoint = point1;
            }
            for (int x = startPoint.x; x <= endPoint.x; x++)
            {
                // if line <1>, <2> and <3> are exist then this Rect is  exist too

                // if there is no line on row x then return false;
                if (!CheckOnRowY(startPoint.x, x, startPoint.y)) return false;
                
                // when line <1> is exist then we check line <2> and <3> to define if there is a valid path for start to end
                if (CheckOnColumnX(startPoint.y, endPoint.y, x) && CheckOnRowY(x, endPoint.x, endPoint.y))
                {
                    Debug.Log("[Pikachu Algorithm][CheckOnRectHorizontal] " + startPoint.ToString() + " -> Column " + x + " -> " + endPoint.ToString());
                    middle1 = m_table[x, point1.y].transform.localPosition; middle2 = m_table[x, point2.y].transform.localPosition;
                    return true;
                }                                    
            }            
            return false;
        }
        private bool CheckOnRectVertical(Point point1, Point point2, out Vector3 middle1, out Vector3 middle2)
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
            middle1 = Vector3.zero; middle2 = Vector3.zero;

            if (point1.y > point2.y) // Must be started from the lower row index on Vertical (because of the FOR loop)
            {
                startPoint = point2;
                endPoint = point1;
            }            
            for (int y = startPoint.y; y <= endPoint.y; y++)
            {
                // if line <1>, <2> and <3> are exist then this Rect is  exist too

                    // if there is no line on row x then return false;
                if (!CheckOnColumnX(startPoint.y, y, startPoint.x)) return false;                

                // when line <1> is exist then we check line <2> and <3> to define if there is a valid path for start to end
                if (CheckOnRowY(startPoint.x, endPoint.x, y) && CheckOnColumnX(y, endPoint.y, endPoint.x))
                {
                    Debug.Log("[Pikachu Algorithm][CheckOnRectVertical] " + startPoint.ToString() + " -> Row " + y + " -> " + endPoint.ToString());
                    middle1 = m_table[point1.x, y].transform.localPosition; middle2 = m_table[point2.x, y].transform.localPosition;
                    return true;
                }
            }
            return false;
        }

        // Expandation Check
        private bool CheckOnHorizontalExpand(Point point1, Point point2, int direction, out Vector3 middle1, out Vector3 middle2)
        {
            /* 
             * Sample for direction = 1
             start_______________<1>_______________<a>
                                                   |
                                                  <2>
                                                   |
                                    end____<3>_____<b>            
            
            * Sample for direction = -1
             <a>_____<1>____start
              |
             <2>
              |
             <b>_____________<3>_____________end

            */
            Point startPoint = point1; Point endPoint = point2;
            middle1 = Vector3.zero; middle2 = Vector3.zero;

            if (point1.x > point2.x) // Must be started from the lower column index on Horizontal (because we have to check the line <1> is exist or not)
            {
                startPoint = point2;
                endPoint = point1;
            }

            int column = endPoint.x;  // The first column (which stands for line <2>) must start from the same column with endPoint
            int row = startPoint.y; // The most important row to check must be the same with the row of startPoint
            if (direction < 0) // If we moving BACKWARD (reducing-type)
            {
                column = startPoint.x; // The first column (which stands for line <2>) must start from the same column with startPoint
                row = endPoint.y; // the most important row to check must be the same with the row of endPoint
            }
            
            if (CheckOnRowY(startPoint.x, endPoint.x, row)) // if line <1> exist
            {
                // while <a> and <b> is not obstacles
                // while we are moving the column from end.x to further, we also guarantee that the line <3> is exist
                while (!m_table[column, startPoint.y].m_IsObstacle && !m_table[column, endPoint.y].m_IsObstacle)
                {
                    // Check if line <2> exist
                    if (CheckOnColumnX(startPoint.y, endPoint.y, column))
                    {
                        Debug.Log("[Pikachu Algorithm][CheckOnHorizontalExpand] " + startPoint.ToString() + " -> Row " + column + " -> " + endPoint.ToString());
                        middle1 = m_table[column, point1.y].transform.localPosition; middle2 = m_table[column, point2.y].transform.localPosition;
                        return true;
                    }

                    column += direction;
                    if (column != Mathf.Clamp(column, 0, m_tableSize.y)) break; 
                }
            }
            return false;
        }
        private bool CheckOnVerticalExpand(Point point1, Point point2, int direction, out Vector3 middle1, out Vector3 middle2)
        {
            /*
             * Sample for direction = 1
               ______<2>_____
               |             |
               |             |
              <1>           <3>
               |             |
               |             |
             start          end

             * Sample for direction = -1
             start          end
               |             |
               |             |
              <1>           <3>
               |             |
               |             |
              <a>____<2>____<b>
                            
            */
            Point startPoint = point1; Point endPoint = point2;
            middle1 = Vector3.zero; middle2 = Vector3.zero;

            if (point1.y > point2.y) // Must be started from the lower row index on Vertical (because we have to check the line <1> is exist or not)
            {
                startPoint = point2;
                endPoint = point1;
            }

            int row = endPoint.y; // The first row (which stands for line <2>) must start from the same row with endPoint
            int column = startPoint.x; // the most important column to check must be the same with the column of startPoint
            if (direction < 0) // If we are moving BACKWARD (reducing-type)
            {
                row = startPoint.y; // the first row (which stand for line <2>) must start from the same row with startPoint
                column = endPoint.x; // the most important column to check must be the same with the column of endPoint
            }

            if (CheckOnColumnX(startPoint.y, endPoint.y, column)) // if line <1> exist
            {
                // while <a> and <b> is not obstacles
                // while we are moving the column from end.x to further, we also guarantee that the line <3> is exist
                while (!m_table[startPoint.x, row].m_IsObstacle && !m_table[endPoint.x, row].m_IsObstacle)
                {
                    // Check if line <2> exist
                    if (CheckOnRowY(startPoint.x, endPoint.x, row))
                    {
                        Debug.Log("[Pikachu Algorithm][CheckOnVerticalExpand] " + startPoint.ToString() + " -> Row " + row + " -> " + endPoint.ToString());
                        middle1 = m_table[point1.x,row].transform.localPosition; middle2 = m_table[point2.x, row].transform.localPosition;
                        return true;
                    }

                    row += direction;
                    if (row != Mathf.Clamp(row, 0, m_tableSize.x)) break;
                }
            }
            return false;
        }

        #endregion
    }
}