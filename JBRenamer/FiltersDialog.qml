import QtQuick
import QtQuick.Controls.Fusion
import QtQuick.Layouts
import QtQuick.Dialogs

Dialog {
    title: "Filter Settings"
    modal: true
    focus: true

    anchors.centerIn: parent
    closePolicy: Popup.CloseOnEscape
    standardButtons: Dialog.Ok | Dialog.Cancel

    onAccepted: function () {
        fileFilters.setItemsInFolders(filterItemsInFolders.checkState === Qt.Checked);
        fileFilters.setFoldersAsItems(filterFoldersAsItems.checkState === Qt.Checked);
        fileFilters.setIncludeSubfolders(filterIncludeSubfolders.checkState === Qt.Checked);
        fileFilters.setIncludeHidden(filterIncludeHiddenItems.checkState === Qt.Checked);
        fileFilters.setSkipRootFolders(filterSkipRootFolders.checkState === Qt.Checked);
        fileFilters.filtersUpdated();
    }
    
    GroupBox {
        title: "Behavior when adding folders:"
        GridLayout {
            columns: 2
            rowSpacing: 5
            columnSpacing: 2

            Image {
                source: "qrc:/assemblies/JBRenamer/icons/filters_files_in_folders.png"
                width: 24; height: 24
                Layout.preferredWidth: 24
                Layout.preferredHeight: 24
            }
            CheckBox {
                Layout.fillWidth: true
                id: filterItemsInFolders
                text: "Add items within folders"
                checkState: (fileFilters.itemsInFolders ? Qt.Checked : Qt.Unchecked)
            }

            Image {
                source: "qrc:/assemblies/JBRenamer/icons/filters_folders_as_files.png"
                width: 24; height: 24
                Layout.preferredWidth: 24
                Layout.preferredHeight: 24
            }
            CheckBox {
                Layout.fillWidth: true
                id: filterFoldersAsItems
                text: "Add folders as items"
                checkState: (fileFilters.foldersAsItems ? Qt.Checked : Qt.Unchecked)
            }

            Image {
                source: "qrc:/assemblies/JBRenamer/icons/filters_subfolders.png"
                width: 24; height: 24
                Layout.preferredWidth: 24
                Layout.preferredHeight: 24
            }
            CheckBox {
                Layout.fillWidth: true
                id: filterIncludeSubfolders
                text: "Include subfolders"
                checkState: (fileFilters.includeSubfolders ? Qt.Checked : Qt.Unchecked)
            }

            Image {
                source: "qrc:/assemblies/JBRenamer/icons/filters_hidden.png"
                width: 24; height: 24
                Layout.preferredWidth: 24
                Layout.preferredHeight: 24
            }
            CheckBox {
                Layout.fillWidth: true
                id: filterIncludeHiddenItems
                text: "Include hidden items"
                checkState: (fileFilters.includeHidden ? Qt.Checked : Qt.Unchecked)
            }

            Image {
                source: "qrc:/assemblies/JBRenamer/icons/filters_skip_root_folders.png"
                width: 24; height: 24
                Layout.preferredWidth: 24
                Layout.preferredHeight: 24
            }
            CheckBox {
                Layout.fillWidth: true
                id: filterSkipRootFolders
                text: "Skip root folders when added as items"
                checkState: (fileFilters.skipRootFolders ? Qt.Checked : Qt.Unchecked)
            }
        }
    }
}
