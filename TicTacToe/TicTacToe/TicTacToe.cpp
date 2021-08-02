#include <iostream>
#include <string>
#include <vector>

using namespace std;

class board {
public:
    enum class Field { EMPTY, X, O };

    using row_t = vector<Field>;
    using column_t = vector<Field>;
    using diagonal_t = vector<Field>;
    using matrix_t = vector<row_t>;

    vector<vector<Field>> matrix;

    board (int n_rows, int n_columns) : matrix(matrix_t(n_rows, row_t(n_columns))) { }

    Field get(int x, int y) const {
        return matrix.at(x).at(y);
    }

    int get_height() const {
        return matrix.size();
    }

    int get_width() const {
        return matrix.at(0).size();
    }

    bool x_valid(int x) const {
        return (x >= 0 && x < get_height());
    }

    bool y_valid(int y) const {
        return (y >= 0 && y < get_width());
    }

    bool position_within_matrix(int x, int y) const {
        return x_valid(x) && y_valid(y);
    }

    bool empty_position(int x, int y) const {
        return get(x, y) == Field::EMPTY;
    }

    row_t get_row(int x) const {
        return matrix.at(x);
    }

    column_t get_column(int x) const {

        int column_len = get_height();
        column_t column(column_len);

        for (int i = 0; i < column_len; i++)
        {
            column[i] = matrix.at(i).at(x);
        }

        return column;
    }

    void get_diagonal(int def_x, int def_y, int x_dir, int y_dir,
        int& curr_field_index, int curr_field_index_inc,
        bool insert_to_beg, diagonal_t& diagonal) const {

        int x = def_x + x_dir;
        int y = def_y + y_dir;
        while (position_within_matrix(x, y))
        {
            if (insert_to_beg)
            {
                diagonal.insert(diagonal.begin(), get(x, y));
            }
            else
            {
                diagonal.push_back(get(x, y));
            }

            x += x_dir;
            y += y_dir;
            curr_field_index += curr_field_index_inc;
        }
    }

    void create_left_half_of_diagonal(int def_x, int def_y, int x_dir, int y_dir,
        int& curr_field_index, diagonal_t& diagonal) const {

        int curr_field_index_increment = 1;
        get_diagonal(def_x, def_y, x_dir, y_dir, curr_field_index, curr_field_index_increment, true, diagonal);
    }

    void create_right_half_of_diagonal(int def_x, int def_y, int x_dir, int y_dir,
        int& curr_field_index, diagonal_t& diagonal) const {

        int curr_field_index_increment = 0; // fields added to the end of vector, no need to increment curr_field_index
        get_diagonal(def_x, def_y, x_dir, y_dir, curr_field_index, curr_field_index_increment, false, diagonal);
    }

    diagonal_t get_top_to_bottom_diagonal(int def_x, int def_y, int& curr_field_index) const {
        diagonal_t diagonal{ get(def_x, def_y) };
        create_left_half_of_diagonal(def_x, def_y, -1, -1, curr_field_index, diagonal);
        create_right_half_of_diagonal(def_x, def_y, 1, 1, curr_field_index, diagonal);
        return diagonal;
    }

    diagonal_t get_bottom_to_top_diagonal(int def_x, int def_y, int& curr_field_index) const {
        diagonal_t diagonal{ get(def_x, def_y) };
        create_left_half_of_diagonal(def_x, def_y, 1, -1, curr_field_index, diagonal);
        create_right_half_of_diagonal(def_x, def_y, -1, 1, curr_field_index, diagonal);
        return diagonal;
    }

    void set(int x, int y, const Field& field) {
        matrix.at(x).at(y) = field;
    }

    bool field_X(int i, int j) const {
        return matrix[i][j] == Field::X;
    }

    bool field_O(int i, int j) const {
        return matrix[i][j] == Field::O;
    }

    void print() const {
        for (int i = 0; i < get_height(); i++)
        {
            for (int j = 0; j < get_width(); j++)
            {
                if (field_X(i, j))
                {
                    cout << "X" << " ";
                }
                else if (field_O(i, j))
                {
                    cout << "O" << " ";
                }
                else //empty field
                {
                    cout << "." << " ";
                }
            }
            cout << endl;
        }
    }
};

class game {
public:
    game(int n_rows, int n_columns) : b(board(n_rows, n_columns)), fields_to_win(5) { }

    void set_fields_to_win(int win_fields) {
        fields_to_win = win_fields;
    }

    void play_game() {
        b.print();

        Field current_field = Field::X; //player 1 starts with X
        bool game_over = false;

        while (!game_over)
        {
            int x, y;
            get_valid_positions(x, y);
            b.set(x, y, current_field);
            game_over = is_game_over(x, y, current_field);
            current_field = set_next_round_field(current_field);
            b.print();
        }

        cout << "GAME OVER";
    }
private:
    using Field = board::Field;

    board b;
    int fields_to_win;

    void get_valid_positions(int& x, int& y) const {
        cin >> x >> y;

        while (!b.position_within_matrix(x, y) || !b.empty_position(x, y)) //forces the player to give valid field position
        {
            cout << "Invalid position given. Choose number 0-" 
                << b.get_height() << " for row and 0-" 
                << b.get_width() << " for column." << endl;

            cin >> x >> y;
        }
    }

    Field set_next_round_field(const Field& current_field) const {
        if (current_field == Field::X)
        {
            return Field::O;
        }
        else
        {
            return Field::X;
        }
    }

    bool same_field(const vector<Field>& curr_vector, const Field& curr_field, int index) const {
        return curr_vector[index] == curr_field;
    }

    int count_same_fields(int default_index, const vector<Field>& curr_vector, const Field& curr_field) const {
        int index = default_index + 1;
        int field_counter = 1;

        while (index < curr_vector.size() && same_field(curr_vector, curr_field, index))
        {
            index++;
            field_counter++;
        }

        if (field_counter < 5)
        {
            index = default_index - 1;
            while (index >= 0 && same_field(curr_vector, curr_field, index))
            {
                index--;
                field_counter++;
            }
        }

        return field_counter;
    }

    bool curr_player_won(int field_counter) const {
        return field_counter >= fields_to_win;
    }

    bool is_game_over(int curr_x, int curr_y, const Field& curr_field) const {
        board::row_t curr_row = b.get_row(curr_x);
        int field_counter = count_same_fields(curr_y, curr_row, curr_field); // check row

        if (!curr_player_won(field_counter))
        {
            board::column_t curr_column = b.get_column(curr_y);
            field_counter = count_same_fields(curr_x, curr_column, curr_field); // check column
        }
        if (!curr_player_won(field_counter))
        {
            int curr_field_index = 0; // to remark position of current field in vector of diagonal positions
            board::diagonal_t t_b_diagonal = b.get_top_to_bottom_diagonal(curr_x, curr_y, curr_field_index);
            field_counter = count_same_fields(curr_field_index, t_b_diagonal, curr_field); //check t_b_diagonal
        }
        if (!curr_player_won(field_counter))
        {
            int curr_field_index = 0; // to remark position of current field in vector of diagonal positions
            board::diagonal_t b_t_diagonal = b.get_bottom_to_top_diagonal(curr_x, curr_y, curr_field_index);
            field_counter = count_same_fields(curr_field_index, b_t_diagonal, curr_field); // check b_t_diagonal
        }

        return curr_player_won(field_counter);
    }
};

void process_fields_to_win_arg(int arg, game& g, int row_count, int column_count) {
    auto fields_to_win = arg;

    if (fields_to_win <= 0)
    {
        cout << "Invalid optional argument. Number of fields needed to win the game must be positive number.";
        throw;
    }
    if (fields_to_win > row_count && fields_to_win > column_count)
    {
        cout << "Invalid optional argument. Number of fields needed to win the game must be within the board dimensions.";
        throw;
    }
    g.set_fields_to_win(fields_to_win);
}

int main(int argc, char** argv)
{
    vector<string> args(argv, argv + argc);

    if (args.size() < 3)
    {
        cout << "Invalid number of arguments. 2 arguments required for board size.";
        throw;
    }

    auto row_count = stoi(args[1]);
    auto column_count = stoi(args[2]);

    if (row_count <= 0 || column_count <= 0)
    {
        cout << "Invalid arguments. 2 arguments representing positive numbers are required.";
        throw;
    }

    game g(row_count, column_count);

    if (args.size() > 3) process_fields_to_win_arg(stoi(args[3]), g, row_count, column_count);
    
    g.play_game();
}