import QtQuick
import QtQuick.Controls.Fusion
import QtQuick.Layouts
import QtQuick.Dialogs

ToolBar {
    Layout.fillWidth: true

    signal fileListUpdated()

    onFileListUpdated: function () {
        fileCount.text = files.count() + " items";
    }

    Connections {
        target: fileFilters

        function onFiltersUpdated() {
            console.log("filtersUpdated");
            filterStateItemsInFolders.opacity = (fileFilters.itemsInFolders ? 1.0 : 0.0);
            filterStateFoldersAsItems.opacity = (fileFilters.foldersAsItems ? 1.0 : 0.0);
            filterStateIncludeSubfolders.opacity = (fileFilters.includeSubfolders ? 1.0 : 0.0);
            filterStateIncludeHidden.opacity = (fileFilters.includeHidden ? 1.0 : 0.0);
            filterStateSkipRootFolders.opacity = (fileFilters.skipRootFolders ? 1.0 : 0.0);
        }
    }

    RowLayout {

        ToolButton {
            id: filterStateItemsInFolders
            icon.source: "qrc:/assemblies/JBRenamer/icons/filters_files_in_folders.png"
            opacity: (fileFilters.itemsInFolders ? 1.0 : 0.0)
            flat: true
            enabled: false
            
            ToolTip.visible: (hovered && (filterStateItemsInFolders.opacity === 1.0))
            ToolTip.text: "Add items within folders"
        }
        ToolButton {
            id: filterStateFoldersAsItems
            icon.source: "qrc:/assemblies/JBRenamer/icons/filters_folders_as_files.png"
            opacity: (fileFilters.foldersAsItems ? 1.0 : 0.0)
            flat: true
            enabled: false

            ToolTip.text: "Add folders as items"
            ToolTip.visible: (hovered && (filterStateFoldersAsItems.opacity === 1.0))
        }
        ToolButton {
            id: filterStateIncludeSubfolders
            icon.source: "qrc:/assemblies/JBRenamer/icons/filters_subfolders.png"
            opacity: (fileFilters.includeSubfolders ? 1.0 : 0.0)
            flat: true
            enabled: false

            ToolTip.text: "Include subfolders"
            ToolTip.visible: (hovered && (filterStateIncludeSubfolders.opacity === 1.0))
        }
        ToolButton {
            id: filterStateIncludeHidden
            icon.source: "qrc:/assemblies/JBRenamer/icons/filters_hidden.png"
            opacity: (fileFilters.includeHiddenItems ? 1.0 : 0.0)
            flat: true
            enabled: false

            ToolTip.text: "Include hidden items"
            ToolTip.visible: (hovered && (filterStateIncludeHidden.opacity === 1.0))
        }
        ToolButton {
            id: filterStateSkipRootFolders
            icon.source: "qrc:/assemblies/JBRenamer/icons/filters_skip_root_folders.png"
            opacity: (fileFilters.skipRootFolders ? 1.0 : 0.0)
            flat: true
            enabled: false

            ToolTip.text: "Skip root folders when added as items"
            ToolTip.visible: (hovered && (filterStateSkipRootFolders.opacity === 1.0))
        }

        ToolSeparator {
        }

        Label {
            id: fileCount
            text: files.count() + " items"

            horizontalAlignment: Qt.AlignLeft
            verticalAlignment: Qt.AlignVCenter
        }
    }
}
