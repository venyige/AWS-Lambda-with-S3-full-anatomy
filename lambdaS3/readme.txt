#################

https://aws.amazon.com/blogs/compute/developing-net-core-aws-lambda-functions/
##################

export PATH=$PATH:/home/linuxbrew/.linuxbrew/bin

sam package \
  --template-file template.yaml \
  --output-template debugging-example.yaml \
  --s3-bucket debugging-example-deploy

  sam deploy \
   --template-file debugging-example.yaml \
   --stack-name DebuggingExample \
   --capabilities CAPABILITY_IAM \
   --region eu-west-1




   ##########
   OR
   https://aws.amazon.com/blogs/compute/introducing-simplified-serverless-application-deplyoment-and-management/
   ##########
   aws cloudformation package --template-file app_spec.yml --output-template-file new_app_spec.yml --s3-bucket <your-bucket-name>

   aws cloudformation deploy --template-file new_app_spec.yml --stack-name <your-stack-name> --capabilities CAPABILITY_IAM