using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GridManager : MonoBehaviour
{
    public int startingBlocks = 5;
    public int spawnEverythMove = 5;

    public Block blockPrefab;

    public Transform self;

    public List<Block> testBlocks;
    public SpriteRenderer[] balls;
    public TextMesh movesDisplay;
    public TextMesh levelDisplay;

    public ParticleSystem particle;

    Block[,] blocks = new Block[5, 5];

    Stack<Block> unused = new Stack<Block>();
    HashSet<Block> used = new HashSet<Block>();

    int movesTillLastSpawn;

    int score; 
    public Text scoreDisplay;

    public GameObject gameEnder;
    public GameObject nextButton;
    public GameObject againButton;

    public UnityEvent OnMoved;
    public UnityEvent OnScored;

    void Awake()
    {
        foreach (var item in testBlocks)
        {
            var blockPosition = item.cachedTransform.localPosition;
            int x = Mathf.RoundToInt(blockPosition.x);
            int y = Mathf.RoundToInt(blockPosition.y);

            blocks[x, y] = item;
        }
    }

    void Start()
    {
        movesTillLastSpawn = spawnEverythMove;
        SpawnInitialBlocks();
        DisplayLevel();
        DisplayScore();
    }

    [ContextMenu(nameof(NextLevel))]
    public void NextLevel()
    {
        Block.level++;
        if (Block.level >= Block.MAX_VALUES.Length)
            EndGame();

        DisplayLevel();
        foreach (var block in used)
        {
            block.Display();
        }
        ResetScore();
        ResetMoves();
        enabled = true;
        gameEnder.SetActive(false);
    }

    void SpawnInitialBlocks()
    {
        for (int i = 0; i < startingBlocks; i++)
        {
            RESTART:
            int x = Random.Range(0, 5);
            int y = Random.Range(0, 5);

            if (blocks[x, y] != null || (x == 2 && y == 2))
                goto RESTART;

            InitializeNewBlock(x, y);
        }
    }

    void DisplayLevel()
    {
        levelDisplay.text = $"{Block.DecimalToBinary(Block.MAX_VALUES[Block.level])} GAME";
    }

    void InitializeNewBlock(int x, int y)
    {
        var block = blocks[x, y] = GetNewBlock();
        block.SetRandom();
        block.gridPosition = new Vector2Int(x, y);
        block.cachedTransform.localPosition = new Vector3(x, y);
        block.Appear();
    }

    bool GetEmptyCoordinate( out Vector2Int coord )
    {
        List<Vector2Int> empties = new List<Vector2Int>();

        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                if (x == 2 && y == 2)
                    continue;

                if (blocks[x, y] == null)
                    empties.Add(new Vector2Int(x, y));
            }
        }

        if (empties.Count == 0)
        {
            coord = default;
            return false;
        } else
        {
            coord = empties[Random.Range(0, empties.Count)];
            return true;
        }
    }

    Block GetNewBlock()
    {
        Block block;

        if (unused.Count > 0)
        {
            block = unused.Pop();
            used.Add(block);
            block.gameObject.SetActive(true);
            return block;
        }

        block = Instantiate(blockPrefab, self, false);
        used.Add(block);
        return block;
    }

    void Update()
    {
        if (!Input.anyKeyDown)
            return;

        if (Input.GetKeyDown(KeyCode.LeftArrow))
            MoveLeft();
        else if (Input.GetKeyDown(KeyCode.RightArrow))
            MoveRight();
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            MoveUp();
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            MoveDown();
        else if(Input.GetKeyDown(KeyCode.A))
            MoveLeft();
        else if (Input.GetKeyDown(KeyCode.D))
            MoveRight();
        else if (Input.GetKeyDown(KeyCode.W))
            MoveUp();
        else if (Input.GetKeyDown(KeyCode.S))
            MoveDown();
        else
            return;

        MarkMove();
    }

    int ballIndex = 2;
    public Color movedColor, ballColor;
    int movesCount = 0;

    void MarkMove()
    {
        movesCount++;
        OnMoved.Invoke();
        DisplayMoves();

        if (movesCount == 11111)
            EndGame();

        movesTillLastSpawn--;
        if (movesTillLastSpawn > 0)
        {
            balls[ballIndex--].color = movedColor;
            return;
        }

        SpawnNewBlock();
        movesTillLastSpawn = spawnEverythMove;
        ballIndex = 2;
        foreach (var ball in balls)
        {
            ball.color = ballColor;
        }
    }

    void DisplayMoves()
    {
        movesDisplay.text = (movesCount).ToString();
    }

    void ResetMoves()
    {
        movesCount = 0;
        DisplayMoves();
    }

    void SpawnNewBlock()
    {
        if (!GetEmptyCoordinate(out Vector2Int coord))
        {
            EndGame();
            return;
        }
        else
        {
            InitializeNewBlock(coord.x, coord.y);
        }
    }

    void EndLevel()
    {
        gameEnder.SetActive(true);
        againButton.SetActive(false);
        nextButton.SetActive(true);
        enabled = false;
    }

    void EndGame()
    {
        gameEnder.SetActive(true);
        againButton.SetActive(true);
        nextButton.SetActive(false);
        enabled = false;
    }

    public void RestartGame()
    {
        SceneManager.UnloadScene(0);
        SceneManager.LoadScene(0);
    }

    void MoveDown()
    {
        for (int y = 1; y < 5; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                var block = blocks[x, y];
                if (block == null)
                    continue;

                if (x == 2 && y == 3)
                    continue;

                if(blocks[x,y-1] != null)
                {
                    Collide(block, blocks[x, y - 1]);
                    continue;
                }

                block.cachedTransform.localPosition = new Vector3(x,y-1);
                block.gridPosition = new Vector2Int(x, y - 1);
                blocks[x, y - 1] = block;
                blocks[x, y] = null;
            }
        }
    }

    void MoveUp()
    {
        for (int y = 3; y >= 0; y--)
        {
            for (int x = 0; x < 5; x++)
            {
                var block = blocks[x, y];
                if (block == null)
                    continue;

                if (x == 2 && y == 1)
                    continue;

                if (blocks[x, y + 1] != null)
                {
                    Collide(block, blocks[x, y + 1]);
                    continue;
                }

                block.cachedTransform.localPosition = new Vector3(x, y + 1);
                block.gridPosition = new Vector2Int(x, y + 1);
                blocks[x, y + 1] = block;
                blocks[x, y] = null;
            }
        }
    }

    void MoveRight()
    {
        for (int x = 3; x >= 0; x--)
        {
            for (int y = 0; y < 5; y++)
            {
                var block = blocks[x, y];
                if (block == null)
                    continue;

                if (x == 1 && y == 2)
                    continue;

                if (blocks[x + 1, y] != null)
                {
                    Collide(block, blocks[x + 1, y]);
                    continue;
                }

                block.cachedTransform.localPosition = new Vector3(x + 1, y);
                block.gridPosition = new Vector2Int(x + 1, y);
                blocks[x + 1, y] = block;
                blocks[x, y] = null;
            }
        }
    }

    void MoveLeft()
    {
        for (int x = 1; x < 5; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                var block = blocks[x, y];
                if (block == null)
                    continue;

                if (x == 3 && y == 2)
                    continue;

                if (blocks[x - 1, y] != null)
                {
                    Collide(block, blocks[x - 1, y]);
                    continue;
                }

                block.cachedTransform.localPosition = new Vector3(x - 1, y);
                block.gridPosition = new Vector2Int(x - 1, y);
                blocks[x - 1, y] = block;
                blocks[x, y] = null;
            }
        }
    }

    void Collide(Block collidingBlock, Block targetBlock)
    {
        int sum = collidingBlock.decimalValue + targetBlock.decimalValue;
        if (sum > Block.MAX_VALUES[Block.level])
            targetBlock.Set(sum - Block.MAX_VALUES[Block.level]);
        else if (sum == Block.MAX_VALUES[Block.level])
        {
            particle.transform.position = targetBlock.cachedTransform.position;
            particle.Play();
            Score();
            RemoveBlock(targetBlock);
        }
        else
            targetBlock.Set(sum);

        RemoveBlock(collidingBlock);
    }

    void ResetScore()
    {
        score = 0;
        DisplayScore();
    }

    void DisplayScore()
    {
        scoreDisplay.text = Block.DecimalToBinary(score);
    }

    void Score()
    {
        OnScored.Invoke();
        score++;
        DisplayScore();

        if (score == Block.MAX_VALUES.Last())
            EndGame();
        else if (score == Block.MAX_VALUES[Block.level])
            EndLevel();
    }

    void RemoveBlock(Block block)
    {
        block.gameObject.SetActive(false);
        blocks[block.gridPosition.x, block.gridPosition.y] = null;
        unused.Push(block);
        used.Remove(block);
    }

    void Move(Vector2 direction)
    {
        foreach (var item in blocks)
        {
            var blockPosition = item.cachedTransform.localPosition;
            Vector3 newPosition = GetNewPosition(direction, blockPosition);

            if (ExceedsBounds(newPosition))
                continue;

            item.cachedTransform.localPosition = newPosition;
        }
    }

    static Vector3 GetNewPosition(Vector2 direction, Vector3 blockPosition)
    {
        return new Vector3(
                        blockPosition.x + direction.x,
                        blockPosition.y + direction.y);
    }

    bool ExceedsBounds(Vector3 position)
    {
        return position.x < 0 || position.y < 0 || position.x > 4 || position.y > 4;

    }
}
