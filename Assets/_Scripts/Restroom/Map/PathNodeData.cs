[System.Serializable]
public class PathNodeData
{
    public string id;
    public int row;
    public float columnPosition;
    public int modifierIndex;
    public string[] connectedToNodes;

    public PathNodeData(int row, float columnPosition, int modifierIndex)
    {
        this.id = System.Guid.NewGuid().ToString();
        this.row = row;
        this.columnPosition = columnPosition;
        this.modifierIndex = modifierIndex;
        this.connectedToNodes = new string[0];
    }
}