using UnityEngine;
using UnityEngine.UI;

namespace Pikachu
{
    public class PikachuManagement : MonoBehaviour
    {
        [Header("Matrix Properties")]
        [SerializeField] GameManagement m_gameManagement;
        [SerializeField] ColorButton[] m_buttonTypeList;
        public Vector2 m_tableSize;   
        
        public ColorButton[,] m_playerTable;
        public ColorButton[,] m_opponentTable;

        private ColorButton m_startObject;
        private ColorButton m_endObject;

        [Header("UI Properties")]        
        [SerializeField] RectTransform m_playerUI;
        [SerializeField] RectTransform m_opponentUI;
            
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
        private const float m_buttonSize = 75f;

        [ContextMenu("Create table")]
        public void CreatePlayerTable()
        {
            int row = (int) m_tableSize.x;
            int column = (int) m_tableSize.y;
            m_playerTable = new ColorButton[row, column];
            // Create random button
            for (int i = 0; i < (row * column) / 2; i++)
            {
                // Create random button type
                int random = Random.Range(0, m_buttonTypeList.Length);

                // Get random button location in table
                int x1 = 0, y1 = 0;
                while (m_playerTable[x1, y1] != null)
                {
                    x1 = Random.Range(0, row);
                    y1 = Random.Range(0, column);
                }
                m_playerTable[x1, y1] = Instantiate(m_buttonTypeList[random], m_playerUI);
                m_playerTable[x1, y1].transform.SetParent(m_playerUI, false);

                int x2 = 0, y2 = 0;
                while (m_playerTable[x2, y2] != null)
                {
                    x2 = Random.Range(0, row);
                    y2 = Random.Range(0, column);
                }
                m_playerTable[x2, y2] = Instantiate(m_buttonTypeList[random], m_playerUI);
                m_playerTable[x2, y2].transform.SetParent(m_playerUI, false);
            }

            // Set position
            float rowStart = - ((m_tableSize.x - 1) / 2) * m_buttonSize; float columnStart = - ((m_tableSize.y - 1) / 2) * m_buttonSize;
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < column; j++)
                {
                    // Set position and location of button
                    m_playerTable[i, j].m_RectTransform.localPosition = new Vector3(columnStart + j * m_buttonSize, rowStart + i * m_buttonSize, 0);
                    m_playerTable[i, j].x = i; m_playerTable[i, j].y = j;
                }
            }
            m_gameManagement.SendTableData();
        }                
        public void CreateOpponentTable(byte[,] buttonCode, Vector2 tableSize)
        {
            // Set position
            float rowStart = -((m_tableSize.x - 1) / 2) * m_buttonSize; float columnStart = -((tableSize.y - 1) / 2) * m_buttonSize;
            // Create table
            m_opponentTable = new ColorButton[(int)tableSize.x, (int)tableSize.y];
            for (int i = 0; i < tableSize.x; i++)
            {
                for (int j = 0; j < tableSize.y; j++)
                {
                    // Create button
                    var button = Instantiate(m_buttonTypeList[buttonCode[i, j]], m_opponentUI);
                    button.GetComponent<ColorButton>().m_RectTransform.localPosition = new Vector3(columnStart + j * m_buttonSize, rowStart + i * m_buttonSize, 0);
                    m_opponentTable[i, j] = button;
                    m_opponentTable[i, j].x = i; m_opponentTable[i, j].y = j;
                }
            }
        }

        public void OnButtonClicked(ColorButton choosenObject)
        {
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
                m_gameManagement.SendPairData(m_startObject, m_endObject);
            }

            // Turn off choosen mode
            m_startObject.transform.GetChild(0).gameObject.SetActive(false);
            m_endObject.transform.GetChild(0).gameObject.SetActive(false);
            
            // Renew object
            m_startObject = null;
            m_endObject = null;
        }
        public void HidePair(ColorButton start, ColorButton end)
        {
            start.m_IsObstacle = false;
            start.m_Image.enabled = false;
            start.GetComponent<Button>().interactable = false;

            end.m_IsObstacle = false;
            end.m_Image.enabled = false;
            end.GetComponent<Button>().interactable = false;
        }
        private bool CheckValidPair(ColorButton button1, ColorButton button2)
        {
            if (button1.color != button2.color) return false;
            button1.m_IsObstacle = false; button2.m_IsObstacle = false;

            // if two pair are on a same column or same row
            if (button1.x == button2.x)
            {
                return CheckOnRowX(button1.y, button2.y, button1.x);
            }
            if (button1.y == button2.y)
            {
                return CheckOnColumnY(button1.x, button2.x, button1.y);
            }

            // Check with Rectangle
            if (CheckOnRectHorizontal(new Point(button1.x, button1.y), new Point(button2.x, button2.y))) return true;
            if (CheckOnRectVertical(new Point(button1.x, button1.y), new Point(button2.x, button2.y))) return true;

            // Expandation Check
                // Horizontal
            if (CheckOnHorizontalExpand(new Point(button1.x, button1.y), new Point(button2.x, button2.y), 1)) return true;
            if (CheckOnHorizontalExpand(new Point(button1.x, button1.y), new Point(button2.x, button2.y),-1)) return true;
            // Vertical
            if (CheckOnVerticalExpand(new Point(button1.x, button1.y), new Point(button2.x, button2.y), 1)) return true;
            if (CheckOnVerticalExpand(new Point(button1.x, button1.y), new Point(button2.x, button2.y),-1)) return true;

            button1.m_IsObstacle = true; button2.m_IsObstacle = true;
            return false;
        }


        #region PikachuAlgorithm

        // Check pair on a same line or same column
        private bool CheckOnRowX(int y1, int y2, int x)
        {
            if (x == 0 || x == m_tableSize.x - 1) return true;            

            int start = Mathf.Min(y1, y2);
            int end = Mathf.Max(y1, y2);

            for (int y = start; y < end; y++)
            {
                // Has object between y1 and y2 on the line x
                if (m_playerTable[x, y].m_IsObstacle)
                {
                    Debug.Log(x + " " + y + " " + m_playerTable[x, y].m_IsObstacle);
                    return false;
                }
            }
            Debug.Log("Check on row X");
            return true;
        }
        private bool CheckOnColumnY(int x1, int x2, int y)
        {
            if (y == 0 || y == m_tableSize.y - 1) return true;            

            int start = Mathf.Min(x1, x2);
            int end = Mathf.Max(x1, x2);

            for (int x = start; x < end; x++)
            {
                // Has object between y1 and y2 on the line x
                if (m_playerTable[x, y].m_IsObstacle)
                {
                    Debug.Log(x + " " + y + " " + m_playerTable[x, y].m_IsObstacle);
                    return false;
                }
            }
            Debug.Log("Check on column Y");
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
                if (CheckOnColumnY(startPoint.x, endPoint.x, y) && CheckOnRowX(y, endPoint.y, endPoint.x))
                {
                    Debug.Log("Check on rect horizontal");
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
                if (CheckOnColumnY(startPoint.x, x, startPoint.y)) return false;
                // when line <1> is exist then we check line <2> and <3> to define if there is a valid path for start to end
                if (CheckOnRowX(startPoint.y, endPoint.y, x) && CheckOnColumnY(x, endPoint.x, endPoint.y))
                {
                    Debug.Log("Check on rect vertical");
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

                while (!m_playerTable[startPoint.x, column].m_IsObstacle && !m_playerTable[endPoint.x, column])
                {
                    if (CheckOnColumnY(startPoint.x, endPoint.x, column))
                    {
                        Debug.Log("Check on horizontal expand");
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
                while (!m_playerTable[row, startPoint.y].m_IsObstacle && !m_playerTable[row, endPoint.y].m_IsObstacle)
                {
                    if (CheckOnRowX(startPoint.y, endPoint.y, row))
                    {
                        Debug.Log("Check on vertical expand");
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