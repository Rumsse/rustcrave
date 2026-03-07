[System.Serializable]
public class PathNodeData
{
    public int row;
    public int column;
    public int modifierIndex;
    public int[] connectedToColumns;

    public PathNodeData(int row, int column, int modifierIndex, int[] connectedToColumns)
    {
        this.row = row;
        this.column = column;
        this.modifierIndex = modifierIndex;
        this.connectedToColumns = connectedToColumns;
    }
}