import tkinter as tk
import random
import time

GRID_WIDTH = 10
GRID_HEIGHT = 20
BLOCK_SIZE = 30
GRID_OFFSET_X = 20
GRID_OFFSET_Y = 60

COLORS = {
    'I': '#00f5ff',
    'O': '#ffff00',
    'T': '#bf00ff',
    'S': '#00ff00',
    'Z': '#ff0000',
    'J': '#0000ff',
    'L': '#ff8000'
}

# Each shape is a 4x4 matrix with 1s marking where the block is
# Using standard SRS-style coordinates
SHAPES = {
    'I': [[0,0,0,0],
          [1,1,1,1],
          [0,0,0,0],
          [0,0,0,0]],
    'O': [[1,1,0,0],
          [1,1,0,0],
          [0,0,0,0],
          [0,0,0,0]],
    'T': [[0,1,0,0],
          [1,1,1,0],
          [0,0,0,0],
          [0,0,0,0]],
    'S': [[0,1,1,0],
          [1,1,0,0],
          [0,0,0,0],
          [0,0,0,0]],
    'Z': [[1,1,0,0],
          [0,1,1,0],
          [0,0,0,0],
          [0,0,0,0]],
    'J': [[1,0,0,0],
          [1,1,1,0],
          [0,0,0,0],
          [0,0,0,0]],
    'L': [[0,0,1,0],
          [1,1,1,0],
          [0,0,0,0],
          [0,0,0,0]]
}

DARK_COLORS = {k: '#' + ''.join(hex(max(0, int(v[i:i+2], 16) - 40))[2:].zfill(2) for i in (1, 3, 5))
               for k, v in COLORS.items()}

class Tetromino:
    def __init__(self):
        self.type = random.choice(list(SHAPES.keys()))
        self.color = COLORS[self.type]
        self.dark_color = DARK_COLORS[self.type]
        self.matrix = [row[:] for row in SHAPES[self.type]]
        self.x = GRID_WIDTH // 2 - 2
        self.y = 0

    def rotate_clockwise(self):
        n = len(self.matrix)
        rotated = [[self.matrix[n-1-j][i] for j in range(n)] for i in range(n)]
        return rotated

    def get_blocks(self):
        blocks = []
        for y, row in enumerate(self.matrix):
            for x, cell in enumerate(row):
                if cell:
                    blocks.append((x, y))
        return blocks

class TetrisGame:
    def __init__(self, root):
        self.root = root
        self.root.title("俄罗斯方块")
        self.root.resizable(False, False)
        self.root.configure(bg='#1a1a2e')

        self.canvas = tk.Canvas(root, width=500, height=650, bg='#1a1a2e', highlightthickness=0)
        self.canvas.pack()

        self.grid = [[None] * GRID_WIDTH for _ in range(GRID_HEIGHT)]
        self.current_piece = Tetromino()
        self.next_piece = Tetromino()
        self.score = 0
        self.level = 1
        self.lines_cleared = 0
        self.game_over = False
        self.paused = False
        self.fall_speed = 500
        self.last_fall = time.time() * 1000
        self.running = True

        self.canvas.create_rectangle(
            GRID_OFFSET_X - 2, GRID_OFFSET_Y - 2,
            GRID_OFFSET_X + GRID_WIDTH * BLOCK_SIZE + 2,
            GRID_OFFSET_Y + GRID_HEIGHT * BLOCK_SIZE + 2,
            outline='#4a4a6a', width=2
        )

        self.draw_texts()
        self.bind_keys()
        self.game_loop()

    def draw_texts(self):
        self.canvas.delete('info')
        self.canvas.create_text(340, 80, text='下一个', fill='#ffffff', font=('Courier New', 14, 'bold'), tags='info')
        self.canvas.create_text(340, 180, text='分数', fill='#ffffff', font=('Courier New', 12), tags='info')
        self.canvas.create_text(340, 200, text=str(self.score), fill='#ffffff', font=('Courier New', 20, 'bold'), tags='info')
        self.canvas.create_text(340, 240, text='等级', fill='#ffffff', font=('Courier New', 12), tags='info')
        self.canvas.create_text(340, 260, text=str(self.level), fill='#ffffff', font=('Courier New', 20, 'bold'), tags='info')

        controls = ['操作说明:', '', '← → 移动', '↓ 加速下落', '空格 快速落地', '↑ 旋转', 'P 暂停', 'Esc 退出']
        for i, line in enumerate(controls):
            self.canvas.create_text(340, 320 + i * 22, text=line, fill='#999999', font=('Courier New', 10), tags='info')

    def bind_keys(self):
        self.root.bind('<Left>', lambda e: self.move(-1))
        self.root.bind('<Right>', lambda e: self.move(1))
        self.root.bind('<Down>', lambda e: self.soft_drop())
        self.root.bind('<Up>', lambda e: self.rotate())
        self.root.bind('<space>', lambda e: self.hard_drop())
        self.root.bind('<p>', lambda e: self.toggle_pause())
        self.root.bind('<P>', lambda e: self.toggle_pause())
        self.root.bind('<r>', lambda e: self.restart())
        self.root.bind('<R>', lambda e: self.restart())
        self.root.bind('<Escape>', lambda e: self.root.quit())
        self.root.bind('a', lambda e: self.move(-1))
        self.root.bind('d', lambda e: self.move(1))
        self.root.bind('s', lambda e: self.soft_drop())
        self.root.bind('w', lambda e: self.rotate())

    def check_collision(self, piece, dx=0, dy=0, matrix=None):
        if matrix is None:
            matrix = piece.matrix
        for y, row in enumerate(matrix):
            for x, cell in enumerate(row):
                if cell:
                    new_x = piece.x + x + dx
                    new_y = piece.y + y + dy
                    if new_x < 0 or new_x >= GRID_WIDTH or new_y >= GRID_HEIGHT:
                        return True
                    if new_y >= 0 and self.grid[new_y][new_x] is not None:
                        return True
        return False

    def lock_piece(self):
        for y, row in enumerate(self.current_piece.matrix):
            for x, cell in enumerate(row):
                if cell:
                    gy = self.current_piece.y + y
                    gx = self.current_piece.x + x
                    if 0 <= gy < GRID_HEIGHT and 0 <= gx < GRID_WIDTH:
                        self.grid[gy][gx] = (self.current_piece.color, self.current_piece.dark_color)
        self.clear_lines()
        self.current_piece = self.next_piece
        self.next_piece = Tetromino()
        if self.check_collision(self.current_piece):
            self.game_over = True

    def clear_lines(self):
        full_rows = [y for y in range(GRID_HEIGHT) if all(self.grid[y][x] is not None for x in range(GRID_WIDTH))]
        for y in reversed(full_rows):
            del self.grid[y]
            self.grid.insert(0, [None] * GRID_WIDTH)
        lines = len(full_rows)
        if lines > 0:
            self.lines_cleared += lines
            points = [0, 100, 300, 500, 800]
            self.score += points[lines] * self.level
            self.level = self.lines_cleared // 10 + 1
            self.fall_speed = max(50, 500 * (0.9 ** (self.level - 1)))

    def move(self, dx):
        if not self.game_over and not self.paused:
            if not self.check_collision(self.current_piece, dx=dx):
                self.current_piece.x += dx

    def rotate(self):
        if not self.game_over and not self.paused:
            new_matrix = self.current_piece.rotate_clockwise()
            if not self.check_collision(self.current_piece, matrix=new_matrix):
                self.current_piece.matrix = new_matrix

    def soft_drop(self):
        if not self.game_over and not self.paused:
            if not self.check_collision(self.current_piece, dy=1):
                self.current_piece.y += 1
                self.score += 1
            else:
                self.lock_piece()

    def hard_drop(self):
        if not self.game_over and not self.paused:
            while not self.check_collision(self.current_piece, dy=1):
                self.current_piece.y += 1
                self.score += 2
            self.lock_piece()

    def toggle_pause(self):
        if not self.game_over:
            self.paused = not self.paused

    def restart(self):
        if self.game_over:
            self.grid = [[None] * GRID_WIDTH for _ in range(GRID_HEIGHT)]
            self.current_piece = Tetromino()
            self.next_piece = Tetromino()
            self.score = 0
            self.level = 1
            self.lines_cleared = 0
            self.game_over = False
            self.paused = False
            self.fall_speed = 500
            self.last_fall = time.time() * 1000
            self.canvas.delete('gameover')
            self.canvas.delete('paused')

    def draw_block(self, x, y, color, dark_color, tag=''):
        if y < 0:
            return
        px = GRID_OFFSET_X + x * BLOCK_SIZE
        py = GRID_OFFSET_Y + y * BLOCK_SIZE
        self.canvas.create_rectangle(px + 1, py + 1, px + BLOCK_SIZE - 1, py + BLOCK_SIZE - 1,
                                     fill=color, outline=dark_color, width=2, tags=tag)

    def draw(self):
        self.canvas.delete('block')
        self.canvas.delete('next')

        for y in range(GRID_HEIGHT):
            for x in range(GRID_WIDTH):
                if self.grid[y][x] is not None:
                    color, dark_color = self.grid[y][x]
                    self.draw_block(x, y, color, dark_color, 'block')

        if not self.game_over:
            for y, row in enumerate(self.current_piece.matrix):
                for x, cell in enumerate(row):
                    if cell:
                        self.draw_block(self.current_piece.x + x, self.current_piece.y + y,
                                       self.current_piece.color, self.current_piece.dark_color, 'block')

        for y, row in enumerate(self.next_piece.matrix):
            for x, cell in enumerate(row):
                if cell:
                    px = 320 + x * BLOCK_SIZE
                    py = 100 + y * BLOCK_SIZE
                    self.canvas.create_rectangle(px + 1, py + 1, px + BLOCK_SIZE - 1, py + BLOCK_SIZE - 1,
                                                 fill=self.next_piece.color, outline=self.next_piece.dark_color, width=2, tags='next')

        self.draw_texts()

        if self.game_over:
            self.canvas.delete('gameover')
            self.canvas.create_rectangle(100, 250, 400, 400, fill='#1a1a2e', outline='#ff0000', width=2, tags='gameover')
            self.canvas.create_text(250, 280, text='游戏结束', fill='#ff0000',
                                   font=('Courier New', 28, 'bold'), tags='gameover')
            self.canvas.create_text(250, 330, text=f'最终分数: {self.score}',
                                   fill='#ffffff', font=('Courier New', 16), tags='gameover')
            self.canvas.create_text(250, 370, text='按 R 重新开始',
                                   fill='#ffffff', font=('Courier New', 12), tags='gameover')
        elif self.paused:
            self.canvas.delete('paused')
            self.canvas.create_rectangle(150, 280, 350, 370, fill='#1a1a2e', outline='#ffffff', width=2, tags='paused')
            self.canvas.create_text(250, 325, text='暂停', fill='#ffffff',
                                   font=('Courier New', 24, 'bold'), tags='paused')

    def game_loop(self):
        if not self.running:
            return

        now = time.time() * 1000
        if not self.paused and not self.game_over:
            if now - self.last_fall > self.fall_speed:
                if not self.check_collision(self.current_piece, dy=1):
                    self.current_piece.y += 1
                else:
                    self.lock_piece()
                self.last_fall = now

        self.draw()
        self.root.after(16, self.game_loop)

    def run(self):
        self.root.mainloop()
        self.running = False

if __name__ == '__main__':
    root = tk.Tk()
    game = TetrisGame(root)
    game.run()
