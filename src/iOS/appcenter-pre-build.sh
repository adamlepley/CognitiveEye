if [ ! -n "$APP_CENTER_SECRET" ]
then
    echo "You need define the APP_CENTER_SECRET variable in App Center"
    exit
fi

APP_CONSTANT_FILE=$APPCENTER_SOURCE_DIRECTORY/src/CongnitiveEye.Forms/Utilities/SecretsUtility.cs

if [ -e "$APP_CONSTANT_FILE" ]
then
    echo "Updating APP_CENTER_SECRET to $APP_CENTER_SECRET in SecretsUtility.cs"
    sed -i '' 's#AppCenterSecret = "[a-z:./]*"#AppCenterSecret = "'$APP_CENTER_SECRET'"#' $APP_CONSTANT_FILE

    echo "File content:"
    cat $APP_CONSTANT_FILE
fi