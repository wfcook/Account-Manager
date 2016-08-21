using System.Windows.Forms;

namespace PokemonGoGUI.Extensions
{
    public static class ComboBoxEx
    {
        public static void SelectOption<T>(this ComboBox cb, T option)
        {
            int i = 0;

            foreach(T item in cb.Items)
            {
                if (option.Equals(item))
                {
                    cb.SelectedIndex = i;

                    break;
                }

                i++;
            }
        }

        public static bool HasNullItem(this ComboBox cb)
        {
            if(cb.SelectedItem == null)
            {
                return true;
            }

            return false;
        }
    }
}
